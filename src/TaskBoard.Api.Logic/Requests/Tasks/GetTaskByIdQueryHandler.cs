using AutoMapper;
using MediatR;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskItemDetailDto?>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    public async Task<TaskItemDetailDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null || task.ProjectId != request.ProjectId)
            return null;

        return _mapper.Map<TaskItemDetailDto>(task);
    }
}
