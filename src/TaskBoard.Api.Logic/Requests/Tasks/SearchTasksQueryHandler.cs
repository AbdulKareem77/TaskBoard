using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class SearchTasksQueryHandler : IRequestHandler<SearchTasksQuery, PagedResult<TaskItemDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public SearchTasksQueryHandler(
        ITaskRepository taskRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<PagedResult<TaskItemDto>> Handle(SearchTasksQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                     ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        // SearchAsync uses parameterized LIKE — never concatenates into SQL
        var result = await _taskRepository.SearchAsync(
            request.Q,
            userId,
            request.ProjectId,
            request.Page,
            request.PageSize);

        var items = _mapper.Map<IEnumerable<TaskItemDto>>(result.Items);
        return new PagedResult<TaskItemDto>(items, result.TotalCount, request.Page, request.PageSize);
    }
}
