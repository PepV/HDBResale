using HDBResale.Application.DTOs;

namespace HDBResale.Application.Interfaces;

public interface IPropertyService
{
    Task<IEnumerable<PropertyListDto>> GetPropertyListAsync(string? search = null, string? town = null, int limit = 100, int offset = 0);
    Task<(IEnumerable<PropertyListDto> Data, int TotalCount)> GetPropertyListWithCountAsync(string? search = null, string? town = null, int limit = 100, int offset = 0);
    Task<IEnumerable<string>> GetPropertyTownsAsync();
}