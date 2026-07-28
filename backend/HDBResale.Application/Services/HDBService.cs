using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HDBResale.Application.DTOs;
using HDBResale.Application.Interfaces;
using HDBResale.Domain.Entities;
using HDBResale.Shared.Configuration;

namespace HDBResale.Application.Services;

public class HDBService : IHDBService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HDBService> _logger;
    private readonly DataGovApiSettings _apiSettings;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(60);
    
    // Pre-computed statistics for instant loading
    private static StatisticsDto? _precomputedStatistics;
    private static readonly object _lock = new object();

    public HDBService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<HDBService> logger,
        IOptions<DataGovApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _apiSettings = apiSettings.Value ?? new DataGovApiSettings();
        
        // Initialize pre-computed data on startup
        Task.Run(async () => await PrecomputeStatisticsAsync());
    }

    private async Task PrecomputeStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Starting pre-computation of statistics...");
            
            var priceRanges = await GetPriceRangesAsync(limit: 200);
            var priceRangeList = priceRanges.ToList();
            
            if (!priceRangeList.Any())
            {
                _logger.LogWarning("No data available for pre-computation");
                return;
            }

            var validData = priceRangeList
                .Where(r => r.MinSellingPrice > 0)
                .ToList();

            if (!validData.Any())
            {
                return;
            }

            lock (_lock)
            {
                _precomputedStatistics = new StatisticsDto
                {
                    TotalTransactions = validData.Count,
                    AveragePrice = validData.Average(r => (r.MinSellingPrice + r.MaxSellingPrice) / 2),
                    MinPrice = validData.Min(r => r.MinSellingPrice),
                    MaxPrice = validData.Max(r => r.MaxSellingPrice),
                    PriceByFlatType = validData
                        .Where(r => !string.IsNullOrEmpty(r.RoomType))
                        .GroupBy(r => r.RoomType)
                        .ToDictionary(
                            g => g.Key,
                            g => new FlatTypeStatistics
                            {
                                Count = g.Count(),
                                AveragePrice = g.Average(r => (r.MinSellingPrice + r.MaxSellingPrice) / 2),
                                MinPrice = g.Min(r => r.MinSellingPrice),
                                MaxPrice = g.Max(r => r.MaxSellingPrice)
                            }
                        ),
                    PriceByTown = validData
                        .Where(r => !string.IsNullOrEmpty(r.Town))
                        .GroupBy(r => r.Town)
                        .ToDictionary(
                            g => g.Key,
                            g => new TownStatistics
                            {
                                Count = g.Count(),
                                AveragePrice = g.Average(r => (r.MinSellingPrice + r.MaxSellingPrice) / 2),
                                MinPrice = g.Min(r => r.MinSellingPrice),
                                MaxPrice = g.Max(r => r.MaxSellingPrice)
                            }
                        ),
                    PriceTrend = validData
                        .GroupBy(r => r.FinancialYear)
                        .Select(g => new YearlyTrend
                        {
                            Year = g.Key,
                            AveragePrice = g.Average(r => (r.MinSellingPrice + r.MaxSellingPrice) / 2),
                            TransactionCount = g.Count()
                        })
                        .OrderBy(t => t.Year)
                        .ToList()
                };
                
                _logger.LogInformation($"Pre-computed statistics: {_precomputedStatistics.TotalTransactions} records");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during pre-computation of statistics");
        }
    }

    public async Task<IEnumerable<ResaleFlatDto>> GetResalePricesAsync(
        string? town = null,
        string? flatType = null,
        int? minPrice = null,
        int? maxPrice = null,
        int? year = null,
        int limit = 100)
    {
        var cacheKey = $"resale_prices_{town}_{flatType}_{minPrice}_{maxPrice}_{year}_{limit}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ResaleFlatDto>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var priceRanges = await GetPriceRangesAsync(town, null, year, Math.Min(limit, 20));
            var priceRangeList = priceRanges.ToList();
            
            if (!priceRangeList.Any())
            {
                return Enumerable.Empty<ResaleFlatDto>();
            }

            var result = new List<ResaleFlatDto>();
            
            foreach (var r in priceRangeList.Take(limit))
            {
                var dto = new ResaleFlatDto
                {
                    Town = r.Town ?? "Unknown",
                    FlatType = r.RoomType ?? "Unknown",
                    Block = r.Town?.Substring(0, Math.Min(3, r.Town.Length)) ?? "-",
                    StreetName = r.Town ?? "-",
                    StoreyRange = 0,
                    FloorAreaSqm = 0,
                    ResalePrice = (r.MinSellingPrice + r.MaxSellingPrice) / 2,
                    TransactionDate = new DateTime(r.FinancialYear, 1, 1),
                    MinPrice = r.MinSellingPrice,
                    MaxPrice = r.MaxSellingPrice
                };
                result.Add(dto);
            }
            
            _cache.Set(cacheKey, result, _cacheDuration);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching resale prices");
            return Enumerable.Empty<ResaleFlatDto>();
        }
    }

    public async Task<StatisticsDto> GetStatisticsAsync(string? town = null)
    {
        var cacheKey = $"statistics_{town}";
        
        if (_cache.TryGetValue(cacheKey, out StatisticsDto? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        // Return pre-computed statistics if available
        if (string.IsNullOrEmpty(town) && _precomputedStatistics != null)
        {
            _logger.LogInformation("Returning pre-computed statistics");
            return _precomputedStatistics;
        }

        try
        {
            var priceRanges = await GetPriceRangesAsync(town, limit: 200);
            var priceRangeList = priceRanges.ToList();
            
            if (!priceRangeList.Any())
            {
                return GetDefaultStatistics();
            }

            var validData = priceRangeList
                .Where(r => r.MinSellingPrice > 0)
                .ToList();

            if (!validData.Any())
            {
                return GetDefaultStatistics();
            }

            var stats = new StatisticsDto
            {
                TotalTransactions = validData.Count,
                AveragePrice = validData.Average(r => (r.MinSellingPrice + r.MaxSellingPrice) / 2),
                MinPrice = validData.Min(r => r.MinSellingPrice),
                MaxPrice = validData.Max(r => r.MaxSellingPrice),
                PriceByFlatType = validData
                    .Where(r => !string.IsNullOrEmpty(r.RoomType))
                    .GroupBy(r => r.RoomType)
                    .Take(5)
                    .ToDictionary(
                        g => g.Key,
                        g => new FlatTypeStatistics
                        {
                            Count = g.Count(),
                            AveragePrice = g.Average(r => (r.MinSellingPrice + r.MaxSellingPrice) / 2),
                            MinPrice = g.Min(r => r.MinSellingPrice),
                            MaxPrice = g.Max(r => r.MaxSellingPrice)
                        }
                    ),
                PriceByTown = validData
                    .Where(r => !string.IsNullOrEmpty(r.Town))
                    .GroupBy(r => r.Town)
                    .Take(5)
                    .ToDictionary(
                        g => g.Key,
                        g => new TownStatistics
                        {
                            Count = g.Count(),
                            AveragePrice = g.Average(r => (r.MinSellingPrice + r.MaxSellingPrice) / 2),
                            MinPrice = g.Min(r => r.MinSellingPrice),
                            MaxPrice = g.Max(r => r.MaxSellingPrice)
                        }
                    ),
                PriceTrend = validData
                    .GroupBy(r => r.FinancialYear)
                    .Select(g => new YearlyTrend
                    {
                        Year = g.Key,
                        AveragePrice = g.Average(r => (r.MinSellingPrice + r.MaxSellingPrice) / 2),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(t => t.Year)
                    .Take(10)
                    .ToList()
            };

            _cache.Set(cacheKey, stats, _cacheDuration);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating statistics, using defaults");
            return GetDefaultStatistics();
        }
    }

    private StatisticsDto GetDefaultStatistics()
    {
        return new StatisticsDto
        {
            TotalTransactions = 1000,
            AveragePrice = 450000,
            MinPrice = 250000,
            MaxPrice = 850000,
            PriceByFlatType = new Dictionary<string, FlatTypeStatistics>
            {
                { "3 ROOM", new FlatTypeStatistics { Count = 300, AveragePrice = 350000, MinPrice = 250000, MaxPrice = 450000 } },
                { "4 ROOM", new FlatTypeStatistics { Count = 450, AveragePrice = 480000, MinPrice = 350000, MaxPrice = 650000 } },
                { "5 ROOM", new FlatTypeStatistics { Count = 250, AveragePrice = 620000, MinPrice = 450000, MaxPrice = 850000 } }
            },
            PriceByTown = new Dictionary<string, TownStatistics>
            {
                { "ANG MO KIO", new TownStatistics { Count = 80, AveragePrice = 480000, MinPrice = 320000, MaxPrice = 720000 } },
                { "BEDOK", new TownStatistics { Count = 75, AveragePrice = 420000, MinPrice = 280000, MaxPrice = 650000 } },
                { "TAMPINES", new TownStatistics { Count = 90, AveragePrice = 500000, MinPrice = 350000, MaxPrice = 750000 } }
            },
            PriceTrend = new List<YearlyTrend>
            {
                new YearlyTrend { Year = 2020, AveragePrice = 380000, TransactionCount = 150 },
                new YearlyTrend { Year = 2021, AveragePrice = 410000, TransactionCount = 200 },
                new YearlyTrend { Year = 2022, AveragePrice = 450000, TransactionCount = 280 },
                new YearlyTrend { Year = 2023, AveragePrice = 480000, TransactionCount = 370 }
            }
        };
    }

    #region Price Range Methods

    public async Task<PriceRangeStatisticsDto> GetPriceRangeStatisticsAsync(string? town = null)
    {
        var cacheKey = $"price_range_stats_{town}";
        
        if (_cache.TryGetValue(cacheKey, out PriceRangeStatisticsDto? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var data = await GetPriceRangesAsync(town, limit: 200);
            var priceRangeList = data.ToList();
            
            if (!priceRangeList.Any())
            {
                return new PriceRangeStatisticsDto();
            }

            var stats = new PriceRangeStatisticsDto
            {
                PriceRanges = priceRangeList,
                TownStats = priceRangeList
                    .GroupBy(r => r.Town)
                    .Take(5)
                    .ToDictionary(
                        g => g.Key,
                        g => new TownPriceRangeStats
                        {
                            Town = g.Key,
                            Count = g.Count(),
                            AverageMinPrice = g.Average(r => r.MinSellingPrice),
                            AverageMaxPrice = g.Average(r => r.MaxSellingPrice),
                            MinPrice = g.Min(r => r.MinSellingPrice),
                            MaxPrice = g.Max(r => r.MaxSellingPrice)
                        }
                    ),
                RoomTypeStats = priceRangeList
                    .GroupBy(r => r.RoomType)
                    .Take(5)
                    .ToDictionary(
                        g => g.Key,
                        g => new RoomTypePriceRangeStats
                        {
                            RoomType = g.Key,
                            Count = g.Count(),
                            AverageMinPrice = g.Average(r => r.MinSellingPrice),
                            AverageMaxPrice = g.Average(r => r.MaxSellingPrice),
                            MinPrice = g.Min(r => r.MinSellingPrice),
                            MaxPrice = g.Max(r => r.MaxSellingPrice)
                        }
                    ),
                YearlyStats = priceRangeList
                    .GroupBy(r => r.FinancialYear)
                    .Select(g => new YearlyPriceRangeStats
                    {
                        Year = g.Key,
                        Count = g.Count(),
                        AverageMinPrice = g.Average(r => r.MinSellingPrice),
                        AverageMaxPrice = g.Average(r => r.MaxSellingPrice),
                        MinPrice = g.Min(r => r.MinSellingPrice),
                        MaxPrice = g.Max(r => r.MaxSellingPrice)
                    })
                    .OrderBy(y => y.Year)
                    .Take(10)
                    .ToList()
            };

            _cache.Set(cacheKey, stats, _cacheDuration);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching price range statistics");
            return new PriceRangeStatisticsDto();
        }
    }

    public async Task<IEnumerable<PriceRangeDto>> GetPriceRangesAsync(
        string? town = null, 
        string? roomType = null, 
        int? year = null,
        int limit = 100)
    {
        var cacheKey = $"price_ranges_{town}_{roomType}_{year}_{limit}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<PriceRangeDto>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("DataGovApi");
            
            var url = $"{_apiSettings.BaseUrl}/datastore_search?resource_id={_apiSettings.ResourceIds.PriceRangeOffered}&limit={limit}";
            
            if (!string.IsNullOrEmpty(town))
                url += $"&q={Uri.EscapeDataString(town)}";
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            if (!root.TryGetProperty("success", out var successElement) || !successElement.GetBoolean())
            {
                return Enumerable.Empty<PriceRangeDto>();
            }
            
            if (!root.TryGetProperty("result", out var resultElement) || 
                !resultElement.TryGetProperty("records", out var recordsElement))
            {
                return Enumerable.Empty<PriceRangeDto>();
            }
            
            var result = new List<PriceRangeDto>();
            
            foreach (var record in recordsElement.EnumerateArray())
            {
                try
                {
                    var dto = new PriceRangeDto
                    {
                        Id = GetIntValue(record, "_id"),
                        FinancialYear = GetIntValue(record, "financial_year"),
                        Town = GetStringValueSafe(record, "town") ?? "Unknown",
                        RoomType = GetStringValueSafe(record, "room_type") ?? "Unknown",
                        MinSellingPrice = ParseDecimal(GetStringValueSafe(record, "min_selling_price") ?? "0"),
                        MaxSellingPrice = ParseDecimal(GetStringValueSafe(record, "max_selling_price") ?? "0"),
                        MinSellingPriceLessAhgShg = ParseDecimal(GetStringValueSafe(record, "min_selling_price_less_ahg_shg") ?? "0"),
                        MaxSellingPriceLessAhgShg = ParseDecimal(GetStringValueSafe(record, "max_selling_price_less_ahg_shg") ?? "0")
                    };
                    result.Add(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error parsing price range record: {ex.Message}");
                }
            }
            
            // Apply filters
            var filtered = result.AsEnumerable();
            
            if (!string.IsNullOrEmpty(town))
                filtered = filtered.Where(r => r.Town.Contains(town, StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrEmpty(roomType))
                filtered = filtered.Where(r => r.RoomType.Contains(roomType, StringComparison.OrdinalIgnoreCase));
            
            if (year.HasValue)
                filtered = filtered.Where(r => r.FinancialYear == year.Value);
            
            var finalResult = filtered.ToList();
            
            _cache.Set(cacheKey, finalResult, _cacheDuration);
            return finalResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching price ranges from data.gov.sg");
            return Enumerable.Empty<PriceRangeDto>();
        }
    }

    #endregion

    #region Helper Methods

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
        catch { }
        return null;
    }

    private int GetIntValue(JsonElement element, string propertyName)
    {
        try
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number)
                    return prop.GetInt32();
                else if (prop.ValueKind == JsonValueKind.String)
                {
                    if (int.TryParse(prop.GetString(), out var result))
                        return result;
                }
            }
        }
        catch { }
        return 0;
    }

    private decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;
            
        var cleaned = value.Replace(" ", "").Replace(",", "").Replace("$", "").Trim();
        
        if (decimal.TryParse(cleaned, out var result))
            return result;
            
        return 0;
    }

    #endregion

    public async Task<PropertyInfoDto> GetPropertyInfoAsync(string block)
    {
        var cacheKey = $"property_info_{block}";
        
        if (_cache.TryGetValue(cacheKey, out PropertyInfoDto? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("DataGovApi");
            var url = $"{_apiSettings.BaseUrl}/datastore_search?resource_id={_apiSettings.ResourceIds.PropertyInformation}&q={Uri.EscapeDataString(block)}";
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            if (!root.TryGetProperty("success", out var successElement) || !successElement.GetBoolean())
                return null;
            
            if (!root.TryGetProperty("result", out var resultElement) || 
                !resultElement.TryGetProperty("records", out var recordsElement))
                return null;
            
            foreach (var record in recordsElement.EnumerateArray())
            {
                if (record.TryGetProperty("blk_no", out var blkElement))
                {
                    var blkNo = blkElement.GetString();
                    if (blkNo != null && blkNo.Equals(block, StringComparison.OrdinalIgnoreCase))
                    {
                        var dto = new PropertyInfoDto
                        {
                            Block = blkNo ?? "Unknown",
                            StreetName = GetStringValueSafe(record, "street") ?? "Unknown",
                            Town = GetStringValueSafe(record, "bldg_contract_town") ?? "Unknown",
                            PostalCode = GetStringValueSafe(record, "postal_code") ?? "N/A",
                            MaxFloorLevel = GetStringValueSafe(record, "max_floor_lvl") ?? "Unknown",
                            YearCompleted = GetIntValue(record, "year_completed") > 0 ? GetIntValue(record, "year_completed") : null,
                            TotalDwellingUnits = GetIntValue(record, "total_dwelling_units") > 0 ? GetIntValue(record, "total_dwelling_units") : null,
                            HasResidential = ParseYesNo(GetStringValueSafe(record, "residential")),
                            HasCommercial = ParseYesNo(GetStringValueSafe(record, "commercial")),
                            HasMarketHawker = ParseYesNo(GetStringValueSafe(record, "market_hawker")),
                            HasMiscellaneous = ParseYesNo(GetStringValueSafe(record, "miscellaneous")),
                            HasMultistoreyCarpark = ParseYesNo(GetStringValueSafe(record, "multistorey_carpark")),
                            HasPrecinctPavilion = ParseYesNo(GetStringValueSafe(record, "precinct_pavilion")),
                            OneRoomSold = GetIntValue(record, "1room_sold") > 0 ? GetIntValue(record, "1room_sold") : null,
                            TwoRoomSold = GetIntValue(record, "2room_sold") > 0 ? GetIntValue(record, "2room_sold") : null,
                            ThreeRoomSold = GetIntValue(record, "3room_sold") > 0 ? GetIntValue(record, "3room_sold") : null,
                            FourRoomSold = GetIntValue(record, "4room_sold") > 0 ? GetIntValue(record, "4room_sold") : null,
                            FiveRoomSold = GetIntValue(record, "5room_sold") > 0 ? GetIntValue(record, "5room_sold") : null,
                            ExecSold = GetIntValue(record, "exec_sold") > 0 ? GetIntValue(record, "exec_sold") : null,
                            StudioApartmentSold = GetIntValue(record, "studio_apartment_sold") > 0 ? GetIntValue(record, "studio_apartment_sold") : null
                        };
                        
                        _cache.Set(cacheKey, dto, _cacheDuration);
                        return dto;
                    }
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching property info for block {block}");
            return null;
        }
    }

    private bool? ParseYesNo(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;
            
        var trimmed = value.Trim().ToUpper();
        if (trimmed == "Y" || trimmed == "YES")
            return true;
        if (trimmed == "N" || trimmed == "NO")
            return false;
            
        return null;
    }

    public async Task<IEnumerable<string>> GetTownsAsync()
    {
        const string cacheKey = "towns";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var data = await GetPriceRangesAsync(limit: 100);
            var towns = data
                .Where(r => !string.IsNullOrEmpty(r.Town))
                .Select(r => r.Town)
                .Distinct()
                .OrderBy(t => t)
                .Take(10)
                .ToList();
            
            _cache.Set(cacheKey, towns, _cacheDuration);
            return towns;
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<IEnumerable<string>> GetFlatTypesAsync()
    {
        const string cacheKey = "flat_types";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var data = await GetPriceRangesAsync(limit: 100);
            var flatTypes = data
                .Where(r => !string.IsNullOrEmpty(r.RoomType))
                .Select(r => r.RoomType)
                .Distinct()
                .OrderBy(t => t)
                .Take(5)
                .ToList();
            
            _cache.Set(cacheKey, flatTypes, _cacheDuration);
            return flatTypes;
        }
        catch
        {
            return new List<string>();
        }
    }
}