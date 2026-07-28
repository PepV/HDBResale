using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HDBResale.Application.DTOs;
using HDBResale.Application.Interfaces;
using HDBResale.Domain.Entities;
using HDBResale.Shared.Configuration;

namespace HDBResale.Application.Services;

public class CEASalespersonService : ICEASalespersonService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CEASalespersonService> _logger;
    private readonly DataGovApiSettings _apiSettings;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public CEASalespersonService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<CEASalespersonService> logger,
        IOptions<DataGovApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _apiSettings = apiSettings.Value ?? new DataGovApiSettings();
    }

    public async Task<IEnumerable<CEASalespersonDto>> GetSalespersonsAsync(
        string? search = null, 
        string? status = null, 
        string? agency = null,
        int limit = 100, 
        int offset = 0)
    {
        var result = await GetSalespersonsWithCountAsync(search, status, agency, limit, offset);
        return result.Data;
    }

    public async Task<(IEnumerable<CEASalespersonDto> Data, int TotalCount)> GetSalespersonsWithCountAsync(
        string? search = null, 
        string? status = null, 
        string? agency = null,
        int limit = 100, 
        int offset = 0)
    {
        var cacheKey = $"cea_salesperson_{search}_{status}_{agency}_{limit}_{offset}";
        
        if (_cache.TryGetValue(cacheKey, out (IEnumerable<CEASalespersonDto> Data, int TotalCount)? cachedData) && cachedData != null)
        {
            return cachedData.Value;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("DataGovApi");
            
            // Fetch more records to allow client-side filtering
            var fetchLimit = 2000;
            var url = $"{_apiSettings.BaseUrl}/datastore_search?resource_id={_apiSettings.ResourceIds.CEASalesperson}&limit={fetchLimit}";
            
            // Only use search parameter for the API
            if (!string.IsNullOrEmpty(search))
                url += $"&q={Uri.EscapeDataString(search)}";
            
            _logger.LogInformation($"Calling CEA Salesperson API: {url}");
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            if (!root.TryGetProperty("success", out var successElement) || !successElement.GetBoolean())
            {
                return (Enumerable.Empty<CEASalespersonDto>(), 0);
            }
            
            if (!root.TryGetProperty("result", out var resultElement))
            {
                return (Enumerable.Empty<CEASalespersonDto>(), 0);
            }
            
            if (!resultElement.TryGetProperty("records", out var recordsElement))
            {
                return (Enumerable.Empty<CEASalespersonDto>(), 0);
            }
            
            var allRecords = new List<CEASalespersonDto>();
            var today = DateTime.Today;
            
            foreach (var record in recordsElement.EnumerateArray())
            {
                try
                {
                    var dto = new CEASalespersonDto();
                    
                    if (record.TryGetProperty("_id", out var idProp))
                        dto.Id = idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt32() : 0;
                    
                    if (record.TryGetProperty("salesperson_name", out var nameProp))
                        dto.SalespersonName = nameProp.GetString() ?? "Unknown";
                    
                    if (record.TryGetProperty("registration_no", out var regNoProp))
                        dto.RegistrationNumber = regNoProp.GetString() ?? "N/A";
                    
                    if (record.TryGetProperty("estate_agent_name", out var agentNameProp))
                        dto.EstateAgentName = agentNameProp.GetString() ?? "Unknown";
                    
                    if (record.TryGetProperty("estate_agent_license_no", out var licenseProp))
                        dto.EstateAgentLicenseNo = licenseProp.GetString() ?? "N/A";
                    
                    if (record.TryGetProperty("registration_start_date", out var startDateProp))
                        dto.RegistrationStartDate = startDateProp.GetString() ?? "N/A";
                    
                    if (record.TryGetProperty("registration_end_date", out var endDateProp))
                        dto.RegistrationEndDate = endDateProp.GetString() ?? "N/A";
                    
                    // Calculate status
                    dto.Status = "Unknown";
                    if (!string.IsNullOrEmpty(dto.RegistrationStartDate) && !string.IsNullOrEmpty(dto.RegistrationEndDate))
                    {
                        if (DateTime.TryParse(dto.RegistrationStartDate, out var start) && 
                            DateTime.TryParse(dto.RegistrationEndDate, out var end))
                        {
                            if (today >= start && today <= end)
                                dto.Status = "Active";
                            else if (today > end)
                                dto.Status = "Expired";
                            else if (today < start)
                                dto.Status = "Pending";
                        }
                    }
                    
                    allRecords.Add(dto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error parsing record: {ex.Message}");
                }
            }
            
            _logger.LogInformation($"Total records fetched: {allRecords.Count}");
            
            // Apply filters client-side
            var filteredRecords = allRecords.AsEnumerable();
            
            // Status filter
            if (!string.IsNullOrEmpty(status))
            {
                filteredRecords = filteredRecords.Where(r => 
                    r.Status != null && r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            
            // Agency filter
            if (!string.IsNullOrEmpty(agency))
            {
                filteredRecords = filteredRecords.Where(r => 
                    r.EstateAgentName != null && 
                    r.EstateAgentName.Equals(agency, StringComparison.OrdinalIgnoreCase));
            }
            
            // Search filter (additional client-side filtering if needed)
            if (!string.IsNullOrEmpty(search))
            {
                filteredRecords = filteredRecords.Where(r => 
                    (r.SalespersonName != null && r.SalespersonName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (r.RegistrationNumber != null && r.RegistrationNumber.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (r.EstateAgentName != null && r.EstateAgentName.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }
            
            var filteredCount = filteredRecords.Count();
            
            // Apply pagination
            var pagedRecords = filteredRecords
                .Skip(offset)
                .Take(limit)
                .ToList();
            
            _logger.LogInformation($"Filtered records: {filteredCount}, Paged: {pagedRecords.Count}");
            
            var returnData = (Data: pagedRecords.AsEnumerable(), TotalCount: filteredCount);
            _cache.Set(cacheKey, returnData, _cacheDuration);
            return returnData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching CEA salesperson data from data.gov.sg");
            return (Enumerable.Empty<CEASalespersonDto>(), 0);
        }
    }

    public async Task<CEASalespersonStatisticsDto> GetStatisticsAsync()
    {
        const string cacheKey = "cea_statistics";
        
        if (_cache.TryGetValue(cacheKey, out CEASalespersonStatisticsDto? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var (data, _) = await GetSalespersonsWithCountAsync(limit: 2000);
            var salespersonList = data.ToList();
            
            var stats = new CEASalespersonStatisticsDto
            {
                TotalCount = salespersonList.Count,
                ActiveCount = salespersonList.Count(s => 
                    s.Status != null && s.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)),
                AgencyDistribution = salespersonList
                    .Where(s => !string.IsNullOrEmpty(s.EstateAgentName))
                    .GroupBy(s => s.EstateAgentName ?? "Unknown")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count()
                    )
            };
            
            _cache.Set(cacheKey, stats, _cacheDuration);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching CEA statistics");
            return new CEASalespersonStatisticsDto();
        }
    }

    public async Task<IEnumerable<string>> GetAgenciesAsync()
    {
        const string cacheKey = "cea_agencies";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<string>? cachedData) && cachedData != null)
        {
            return cachedData;
        }

        try
        {
            var (data, _) = await GetSalespersonsWithCountAsync(limit: 2000);
            var agencies = data
                .Where(s => !string.IsNullOrEmpty(s.EstateAgentName))
                .Select(s => s.EstateAgentName ?? "Unknown")
                .Distinct()
                .OrderBy(a => a)
                .ToList();
            
            _cache.Set(cacheKey, agencies, _cacheDuration);
            return agencies;
        }
        catch
        {
            return new List<string>();
        }
    }
}