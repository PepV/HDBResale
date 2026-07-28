using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HDBResale.Application.DTOs;
using HDBResale.Application.Interfaces;
using HDBResale.Shared.Configuration;

namespace HDBResale.Application.Services;

public class PropertyService : IPropertyService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PropertyService> _logger;
    private readonly DataGovApiSettings _apiSettings;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public PropertyService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<PropertyService> logger,
        IOptions<DataGovApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _apiSettings = apiSettings.Value ?? new DataGovApiSettings();
    }

    public async Task<(IEnumerable<PropertyListDto> Data, int TotalCount)> GetPropertyListWithCountAsync(
        string? search = null, 
        string? town = null, 
        int limit = 100, 
        int offset = 0)
    {
        var cacheKey = $"property_list_{search}_{town}_{limit}_{offset}";
        
        if (_cache.TryGetValue(cacheKey, out (IEnumerable<PropertyListDto> Data, int TotalCount)? cachedData) && cachedData != null)
        {
            return cachedData.Value;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("DataGovApi");
            
            // Build URL with filters
            var url = $"{_apiSettings.BaseUrl}/datastore_search?resource_id={_apiSettings.ResourceIds.PropertyInformation}&limit={limit}&offset={offset}";
            
            if (!string.IsNullOrEmpty(search))
                url += $"&q={Uri.EscapeDataString(search)}";
            
            if (!string.IsNullOrEmpty(town))
                url += $"&q={Uri.EscapeDataString(town)}";
            
            _logger.LogInformation($"Calling property API: {url}");
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            if (!root.TryGetProperty("success", out var successElement) || !successElement.GetBoolean())
            {
                return (Enumerable.Empty<PropertyListDto>(), 0);
            }
            
            if (!root.TryGetProperty("result", out var resultElement))
            {
                return (Enumerable.Empty<PropertyListDto>(), 0);
            }
            
            // Get total count
            var totalCount = 0;
            if (resultElement.TryGetProperty("total", out var totalElement))
            {
                totalCount = totalElement.GetInt32();
            }
            else if (resultElement.TryGetProperty("_full_count", out var fullCountElement))
            {
                totalCount = fullCountElement.GetInt32();
            }
            
            if (!resultElement.TryGetProperty("records", out var recordsElement))
            {
                return (Enumerable.Empty<PropertyListDto>(), totalCount);
            }
            
            var result = new List<PropertyListDto>();
            
            foreach (var record in recordsElement.EnumerateArray())
            {
                try
                {
                    var dto = new PropertyListDto
                    {
                        Id = GetIntValue(record, "_id"),
                        Block = GetStringValueSafe(record, "blk_no") ?? "Unknown",
                        Street = GetStringValueSafe(record, "street") ?? "Unknown",
                        Town = GetStringValueSafe(record, "bldg_contract_town") ?? "Unknown",
                        MaxFloorLevel = GetStringValueSafe(record, "max_floor_lvl") ?? "Unknown",
                        YearCompleted = GetStringValueSafe(record, "year_completed") ?? "Unknown",
                        Residential = GetStringValueSafe(record, "residential") ?? "N",
                        Commercial = GetStringValueSafe(record, "commercial") ?? "N",
                        MarketHawker = GetStringValueSafe(record, "market_hawker") ?? "N",
                        Miscellaneous = GetStringValueSafe(record, "miscellaneous") ?? "N",
                        MultistoreyCarpark = GetStringValueSafe(record, "multistorey_carpark") ?? "N",
                        PrecinctPavilion = GetStringValueSafe(record, "precinct_pavilion") ?? "N",
                        TotalDwellingUnits = GetStringValueSafe(record, "total_dwelling_units") ?? "0",
                        OneRoomSold = GetStringValueSafe(record, "1room_sold") ?? "0",
                        TwoRoomSold = GetStringValueSafe(record, "2room_sold") ?? "0",
                        ThreeRoomSold = GetStringValueSafe(record, "3room_sold") ?? "0",
                        FourRoomSold = GetStringValueSafe(record, "4room_sold") ?? "0",
                        FiveRoomSold = GetStringValueSafe(record, "5room_sold") ?? "0",
                        ExecSold = GetStringValueSafe(record, "exec_sold") ?? "0",
                        StudioApartmentSold = GetStringValueSafe(record, "studio_apartment_sold") ?? "0"
                    };
                    result.Add(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error parsing property record: {ex.Message}");
                }
            }
            
            _logger.LogInformation($"Retrieved {result.Count} property records, Total: {totalCount}");
            
            var returnData = (Data: result.AsEnumerable(), TotalCount: totalCount);
            _cache.Set(cacheKey, returnData, _cacheDuration);
            return returnData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching property list from data.gov.sg");
            return (Enumerable.Empty<PropertyListDto>(), 0);
        }
    }

    public async Task<IEnumerable<PropertyListDto>> GetPropertyListAsync(string? search = null, string? town = null, int limit = 100, int offset = 0)
    {
        var result = await GetPropertyListWithCountAsync(search, town, limit, offset);
        return result.Data;
    }

    public async Task<IEnumerable<string>> GetPropertyTownsAsync()
    {
        const string cacheKey = "property_towns";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var data = await GetPropertyListAsync(limit: 1000);
            var towns = data
                .Where(r => !string.IsNullOrEmpty(r.Town))
                .Select(r => r.Town)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            
            _cache.Set(cacheKey, towns, _cacheDuration);
            return towns;
        }
        catch
        {
            return new List<string>();
        }
    }

    private string? GetStringValueSafe(JsonElement element, string propertyName)
    {
        try
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                return prop.ValueKind switch
                {
                    JsonValueKind.String => prop.GetString(),
                    JsonValueKind.Number => prop.GetRawText(),
                    JsonValueKind.True => "Y",
                    JsonValueKind.False => "N",
                    JsonValueKind.Null => null,
                    _ => prop.GetRawText()
                };
            }
        }
        catch
        {
            // If any error occurs, return null
        }
        return null;
    }

    private int GetIntValue(JsonElement element, string propertyName)
    {
        try
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number)
                {
                    return prop.GetInt32();
                }
                else if (prop.ValueKind == JsonValueKind.String)
                {
                    if (int.TryParse(prop.GetString(), out var result))
                        return result;
                }
            }
        }
        catch
        {
            // If any error occurs, return 0
        }
        return 0;
    }
}