namespace HDBResale.Application.DTOs;

public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; }
    public string Username { get; set; }
    public DateTime ExpiresAt { get; set; }
}
