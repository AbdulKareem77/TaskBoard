namespace TaskBoard.Api.Logic.Models;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, IEnumerable<string> Roles);
