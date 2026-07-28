using Microsoft.AspNetCore.Mvc;
using HDBResale.Application.Interfaces;
using HDBResale.Application.DTOs;

namespace HDBResale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto login)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest(new { success = false, message = "Username and password are required" });
            }

            if (!_authService.ValidateCredentials(login.Username, login.Password))
            {
                return Unauthorized(new { success = false, message = "Invalid username or password" });
            }

            var token = _authService.GenerateToken(login.Username);
            
            return Ok(new 
            { 
                success = true, 
                data = new AuthResponseDto
                {
                    Token = token,
                    Username = login.Username,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { success = false, message = "An error occurred during login" });
        }
    }
}
