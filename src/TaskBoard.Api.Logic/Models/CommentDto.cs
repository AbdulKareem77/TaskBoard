namespace TaskBoard.Api.Logic.Models;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public Guid UserId { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
}
