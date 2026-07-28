using HDBResale.Application.DTOs;

namespace HDBResale.Application.Interfaces;

public interface IHDBService
{
    Task<IEnumerable<ResaleFlatDto>> GetResalePricesAsync(
        string? town = null,
        string? flatType = null,
        int? minPrice = null,
        int? maxPrice = null,
        int? year = null,
        int limit = 100);
    
    Task<StatisticsDto> GetStatisticsAsync(string? town = null);
    
    Task<PropertyInfoDto> GetPropertyInfoAsync(string block);
    
    Task<IEnumerable<string>> GetTownsAsync();
    
    Task<IEnumerable<string>> GetFlatTypesAsync();
    
    // New methods for price ranges
    Task<PriceRangeStatisticsDto> GetPriceRangeStatisticsAsync(string? town = null);
    Task<IEnumerable<PriceRangeDto>> GetPriceRangesAsync(string? town = null, string? roomType = null, int? year = null, int limit = 100);
}