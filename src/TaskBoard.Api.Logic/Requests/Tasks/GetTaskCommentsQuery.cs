using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class GetTaskCommentsQuery : IRequest<IEnumerable<CommentDto>>
{
    public Guid ProjectId { get; init; }
    public Guid TaskId { get; init; }
}
