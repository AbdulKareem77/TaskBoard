using FluentAssertions;
using TaskBoard.Api.Logic.Requests.Auth;
using Xunit;

namespace TaskBoard.Tests.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCredentials_ShouldPassValidation()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "admin@taskboard.local",
            Password = "Password123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "",
            Password = "Password123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "not-an-email",
            Password = "Password123!"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(LoginCommand.Email) &&
            e.ErrorMessage.Contains("valid email"));
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "admin@taskboard.local",
            Password = ""
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
