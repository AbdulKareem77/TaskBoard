using AutoMapper;
using MediatR;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedResult<TaskItemDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetTasksQueryHandler(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<TaskItemDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var result = await _taskRepository.GetByProjectIdAsync(
            request.ProjectId,
            request.Page,
            request.PageSize,
            request.Status);

        var items = _mapper.Map<IEnumerable<TaskItemDto>>(result.Items);
        return new PagedResult<TaskItemDto>(items, result.TotalCount, request.Page, request.PageSize);
    }
}
