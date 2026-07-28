using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;
using HDBResale.Application.Services;
using HDBResale.Shared.Configuration;

namespace HDBResale.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly Mock<ILogger<AuthService>> _loggerMock;

    public AuthServiceTests()
    {
        _loggerMock = new Mock<ILogger<AuthService>>();
        
        var jwtSettings = new JwtSettings
        {
            Key = "YourSuperSecretKeyThatIsAtLeast32CharactersLongAndSecure!",
            Issuer = "HDBResaleAPI",
            Audience = "HDBResaleClient",
            ExpiryInMinutes = 60
        };
        
        var options = Options.Create(jwtSettings);
        _authService = new Application.Services.AuthService(options, _loggerMock.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken_WhenUsernameIsValid()
    {
        // Arrange
        var username = "admin";

        // Act
        var token = _authService.GenerateToken(username);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Length.Should().Be(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateToken_ShouldThrowException_WhenKeyIsMissing()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Key = null!,
            Issuer = "HDBResaleAPI",
            Audience = "HDBResaleClient",
            ExpiryInMinutes = 60
        };
        var options = Options.Create(jwtSettings);
        var authService = new AuthService(options, _loggerMock.Object);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            authService.GenerateToken("admin"));
        exception.Message.Should().Contain("JWT Key is not configured");
    }

    [Fact]
    public void ValidateToken_ShouldReturnTrue_WhenTokenIsValid()
    {
        // Arrange
        var token = _authService.GenerateToken("admin");

        // Act
        var result = _authService.ValidateToken(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_ShouldReturnFalse_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _authService.ValidateToken(invalidToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_ShouldReturnFalse_WhenTokenIsEmpty()
    {
        // Act
        var result = _authService.ValidateToken("");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("admin", "admin123", true)]
    [InlineData("user", "user123", true)]
    [InlineData("admin", "wrongpassword", false)]
    [InlineData("nonexistent", "password", false)]
    public void ValidateCredentials_ShouldValidateCorrectly(string username, string password, bool expected)
    {
        // Act
        var result = _authService.ValidateCredentials(username, password);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("admin", "Admin")]
    [InlineData("user", "User")]
    public void GetUserRole_ShouldReturnCorrectRole(string username, string expectedRole)
    {
        // Act
        var role = _authService.GetUserRole(username);

        // Assert
        role.Should().Be(expectedRole);
    }

    [Fact]
    public void GetUserRole_ShouldReturnUser_WhenUserNotFound()
    {
        // Act
        var role = _authService.GetUserRole("nonexistent");

        // Assert
        role.Should().Be("User");
    }
}