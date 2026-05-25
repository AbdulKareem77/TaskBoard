using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using TaskBoard.Api.Logic.Models;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.Api.Logic.Requests.Dashboard;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;

    public GetDashboardQueryHandler(
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userIdStr = user?.FindFirst("sub")?.Value
                     ?? user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var rolesClaimValue = user?.FindFirst("roles")?.Value ?? string.Empty;
        var isAdmin = rolesClaimValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Any(r => r.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase));

        // Step 1: Get user's projects
        var projects = (await _projectRepository.GetByUserIdAsync(userId, isAdmin)).ToList();
        var projectIds = projects.Select(p => p.Id).ToList();

        if (!projectIds.Any())
        {
            return new DashboardDto(
                new MyTasksDto(
                    Enumerable.Empty<TaskItemDto>(),
                    Enumerable.Empty<TaskItemDto>(),
                    Enumerable.Empty<TaskItemDto>(),
                    Enumerable.Empty<TaskItemDto>()),
                Enumerable.Empty<RecentActivityDto>(),
                Enumerable.Empty<ProjectSummaryDto>());
        }

        // Step 2: Load ALL tasks in ONE query — avoids N+1
        var allTasks = (await _taskRepository.GetByProjectIdsAsync(projectIds)).ToList();

        // Step 3: Filter tasks assigned to current user
        var myTasks = allTasks
            .Where(t => t.Assignees.Any(a => a.UserId == userId))
            .ToList();

        // Step 4: Group by status
        var myTaskDtos = _mapper.Map<IEnumerable<TaskItemDto>>(myTasks).ToList();
        var myTasksDto = new MyTasksDto(
            Todo: myTaskDtos.Where(t => t.Status == "Todo"),
            InProgress: myTaskDtos.Where(t => t.Status == "InProgress"),
            Review: myTaskDtos.Where(t => t.Status == "Review"),
            Done: myTaskDtos.Where(t => t.Status == "Done"));

        // Step 5: Recent activity from task history (across all project tasks)
        var recentActivity = allTasks
            .SelectMany(t => t.History.Select(h => new RecentActivityDto(
                TaskTitle: t.Title,
                ProjectName: t.ProjectName ?? string.Empty,
                Action: h.Action,
                UserName: h.UserName,
                Date: h.DateCreated)))
            .OrderByDescending(a => a.Date)
            .Take(20)
            .ToList();

        // Step 6: Project summaries
        var projectSummaries = projects.Select(p =>
        {
            var projectTasks = allTasks.Where(t => t.ProjectId == p.Id).ToList();
            return new ProjectSummaryDto(
                ProjectId: p.Id,
                ProjectName: p.Name,
                TotalTasks: projectTasks.Count,
                CompletedTasks: projectTasks.Count(t => t.Status == "Done"));
        }).ToList();

        return new DashboardDto(myTasksDto, recentActivity, projectSummaries);
    }
}
