namespace TaskBoard.Api.Logic.Models;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string OwnerName,
    int MemberCount,
    int TaskCount,
    bool IsArchived,
    DateTime DateCreated);

public record ProjectDetailDto(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    string OwnerName,
    bool IsArchived,
    IEnumerable<ProjectMemberDto> Members,
    DateTime DateCreated);

public record ProjectMemberDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role);
