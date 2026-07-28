using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDBResale.Application.Interfaces;
using HDBResale.Application.DTOs;

namespace HDBResale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly ILogger<PropertyController> _logger;

    public PropertyController(IPropertyService propertyService, ILogger<PropertyController> logger)
    {
        _propertyService = propertyService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetPropertyList(
        [FromQuery] string? search = null,
        [FromQuery] string? town = null,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0)
    {
        try
        {
            _logger.LogInformation($"Getting property list with search: {search}, town: {town}, limit: {limit}, offset: {offset}");
            
            var (data, totalCount) = await _propertyService.GetPropertyListWithCountAsync(search, town, limit, offset);
            
            return Ok(new { 
                success = true, 
                data = data, 
                count = data.Count(),
                total = totalCount,
                limit = limit,
                offset = offset
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving property list");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving property list: " + ex.Message });
        }
    }

    [HttpGet("towns")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTowns()
    {
        try
        {
            var result = await _propertyService.GetPropertyTownsAsync();
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving property towns");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving towns: " + ex.Message });
        }
    }
}