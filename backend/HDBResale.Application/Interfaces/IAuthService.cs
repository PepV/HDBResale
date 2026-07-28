namespace HDBResale.Application.Interfaces;

public interface IAuthService
{
    string GenerateToken(string username);
    bool ValidateToken(string token);
    bool ValidateCredentials(string username, string password);
}
