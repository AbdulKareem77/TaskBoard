namespace TaskBoard.Api.Logic.Models;

public record RecentActivityDto(
    string TaskTitle,
    string ProjectName,
    string Action,
    string UserName,
    DateTime Date);

public record ProjectSummaryDto(
    Guid ProjectId,
    string ProjectName,
    int TotalTasks,
    int CompletedTasks);

public record MyTasksDto(
    IEnumerable<TaskItemDto> Todo,
    IEnumerable<TaskItemDto> InProgress,
    IEnumerable<TaskItemDto> Review,
    IEnumerable<TaskItemDto> Done);

public record DashboardDto(
    MyTasksDto MyTasks,
    IEnumerable<RecentActivityDto> RecentActivity,
    IEnumerable<ProjectSummaryDto> ProjectSummaries);
