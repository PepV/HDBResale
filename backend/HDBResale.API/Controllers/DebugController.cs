using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HDBResale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DebugController> _logger;

    public DebugController(IHttpClientFactory httpClientFactory, ILogger<DebugController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("check-cea")]
    public async Task<IActionResult> CheckCEA()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("DataGovApi");
            var url = "https://data.gov.sg/api/action/datastore_search?resource_id=d_07c63be0f37e6e59c07a4ddc2fd87fcb&limit=5";
            
            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            
            var result = new
            {
                success = root.TryGetProperty("success", out var s) && s.GetBoolean(),
                total = root.TryGetProperty("result", out var r) && r.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                fields = new List<object>(),
                sampleRecords = new List<object>()
            };
            
            if (root.TryGetProperty("result", out var resultElement))
            {
                // Get fields
                if (resultElement.TryGetProperty("fields", out var fieldsElement))
                {
                    foreach (var field in fieldsElement.EnumerateArray())
                    {
                        var fieldName = field.TryGetProperty("id", out var id) ? id.GetString() : "unknown";
                        var fieldType = field.TryGetProperty("type", out var type) ? type.GetString() : "unknown";
                        result.fields.Add(new { Name = fieldName, Type = fieldType });
                    }
                }
                
                // Get sample records
                if (resultElement.TryGetProperty("records", out var recordsElement))
                {
                    var count = 0;
                    foreach (var record in recordsElement.EnumerateArray())
                    {
                        if (count >= 3) break;
                        var recordData = new Dictionary<string, object>();
                        foreach (var prop in record.EnumerateObject())
                        {
                            recordData[prop.Name] = prop.Value.ToString();
                        }
                        result.sampleRecords.Add(recordData);
                        count++;
                    }
                }
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}