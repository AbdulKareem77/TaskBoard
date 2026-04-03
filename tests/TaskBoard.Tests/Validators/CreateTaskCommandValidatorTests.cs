using FluentAssertions;
using TaskBoard.Api.Logic.Requests.Tasks;
using Xunit;

namespace TaskBoard.Tests.Validators;

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPassValidation()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = "Implement login feature",
            Description = "Add login functionality using JWT",
            Status = "Todo",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyTitle_ShouldFailWithMessage()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = "",
            Status = "Todo"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTaskCommand.Title));
    }

    [Fact]
    public void Validate_TitleExceedingMaxLength_ShouldFail()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = new string('A', 301), // 301 chars, max is 300
            Status = "Todo"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskCommand.Title) &&
            e.ErrorMessage.Contains("300"));
    }

    [Fact]
    public void Validate_PastDueDate_ShouldFail()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = "Valid Title",
            DueDate = DateTime.UtcNow.AddDays(-1) // Past date
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskCommand.DueDate));
    }

    [Fact]
    public void Validate_InvalidStatus_ShouldFail()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            ProjectId = Guid.NewGuid(),
            Title = "Valid Title",
            Status = "InvalidStatus"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskCommand.Status));
    }
}
