using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HDBResale.Application.Interfaces;

namespace HDBResale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CEASalespersonController : ControllerBase
{
    private readonly ICEASalespersonService _ceaService;
    private readonly ILogger<CEASalespersonController> _logger;

    public CEASalespersonController(ICEASalespersonService ceaService, ILogger<CEASalespersonController> logger)
    {
        _ceaService = ceaService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetSalespersons(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? agency = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            _logger.LogInformation($"Getting CEA salespersons with search: {search}, status: {status}, agency: {agency}, limit: {limit}, offset: {offset}");
            
            var (data, totalCount) = await _ceaService.GetSalespersonsWithCountAsync(search, status, agency, limit, offset);
            
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
            _logger.LogError(ex, "Error retrieving CEA salespersons");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving salespersons: " + ex.Message });
        }
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var result = await _ceaService.GetStatisticsAsync();
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CEA statistics");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving statistics: " + ex.Message });
        }
    }

    [HttpGet("agencies")]
    public async Task<IActionResult> GetAgencies()
    {
        try
        {
            var result = await _ceaService.GetAgenciesAsync();
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving CEA agencies");
            return StatusCode(500, new { success = false, message = "An error occurred while retrieving agencies: " + ex.Message });
        }
    }
}