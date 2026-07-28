using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDBResale.Application.Interfaces;

namespace HDBResale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HDBController : ControllerBase
{
    private readonly IHDBService _hdbService;
    private readonly ILogger<HDBController> _logger;

    public HDBController(IHDBService hdbService, ILogger<HDBController> logger)
    {
        _hdbService = hdbService;
        _logger = logger;
    }

    [HttpGet("resale-prices")]
    public async Task<IActionResult> GetResalePrices(
        [FromQuery] string? town = null,
        [FromQuery] string? flatType = null,
        [FromQuery] int? minPrice = null,
        [FromQuery] int? maxPrice = null,
        [FromQuery] int? year = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            var result = await _hdbService.GetResalePricesAsync(town, flatType, minPrice, maxPrice, year, limit);
            return Ok(new { success = true, data = result, count = result.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resale prices");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving data: " + ex.Message });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] string? town = null)
    {
        try
        {
            var result = await _hdbService.GetStatisticsAsync(town);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving statistics: " + ex.Message });
        }
    }

    [HttpGet("price-ranges")]
    public async Task<IActionResult> GetPriceRanges(
        [FromQuery] string? town = null,
        [FromQuery] string? roomType = null,
        [FromQuery] int? year = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            var result = await _hdbService.GetPriceRangesAsync(town, roomType, year, limit);
            return Ok(new { success = true, data = result, count = result.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price ranges");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving price ranges: " + ex.Message });
        }
    }

    [HttpGet("price-range-statistics")]
    public async Task<IActionResult> GetPriceRangeStatistics([FromQuery] string? town = null)
    {
        try
        {
            var result = await _hdbService.GetPriceRangeStatisticsAsync(town);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price range statistics");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving price range statistics: " + ex.Message });
        }
    }

    [HttpGet("property-info/{block}")]
    public async Task<IActionResult> GetPropertyInfo(string block)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(block))
                return BadRequest(new { success = false, message = "Block number is required" });

            var result = await _hdbService.GetPropertyInfoAsync(block);
            
            if (result == null)
                return NotFound(new { success = false, message = $"Property info not found for block {block}" });
                
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving property info for block {block}");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving property information: " + ex.Message });
        }
    }

    [HttpGet("towns")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTowns()
    {
        try
        {
            var result = await _hdbService.GetTownsAsync();
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving towns");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving towns: " + ex.Message });
        }
    }

    [HttpGet("flat-types")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFlatTypes()
    {
        try
        {
            var result = await _hdbService.GetFlatTypesAsync();
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving flat types");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving flat types: " + ex.Message });
        }
    }
}