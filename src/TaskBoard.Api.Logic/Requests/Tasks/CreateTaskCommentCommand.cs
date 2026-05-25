using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class CreateTaskCommentCommand : IRequest<CommentDto>
{
    public Guid ProjectId { get; init; }
    public Guid TaskId { get; init; }
    public string Content { get; init; } = string.Empty;
}
