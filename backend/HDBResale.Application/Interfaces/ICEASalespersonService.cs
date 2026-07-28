using HDBResale.Application.DTOs;

namespace HDBResale.Application.Interfaces;

public interface ICEASalespersonService
{
    Task<IEnumerable<CEASalespersonDto>> GetSalespersonsAsync(
        string? search = null, 
        string? status = null, 
        string? agency = null,
        int limit = 100, 
        int offset = 0);
    
    Task<(IEnumerable<CEASalespersonDto> Data, int TotalCount)> GetSalespersonsWithCountAsync(
        string? search = null, 
        string? status = null, 
        string? agency = null,
        int limit = 100, 
        int offset = 0);
    
    Task<CEASalespersonStatisticsDto> GetStatisticsAsync();
    Task<IEnumerable<string>> GetAgenciesAsync();
}