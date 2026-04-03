using MediatR;
using TaskBoard.Api.Logic.Shared.Authorization;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class DeleteTaskCommand : IRequest<bool>, IRequirePermission
{
    public Guid TaskId { get; init; }
    public Guid ProjectId { get; init; }
    public string Permission => "TaskDelete";
}
