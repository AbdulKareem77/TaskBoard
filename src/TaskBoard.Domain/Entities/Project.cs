namespace TaskBoard.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OwnerId { get; set; }
    public bool IsArchived { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public List<ProjectMember> Members { get; set; } = new();
    // Denormalized from query
    public string? OwnerName { get; set; }
    public int MemberCount { get; set; }
    public int TaskCount { get; set; }
}
