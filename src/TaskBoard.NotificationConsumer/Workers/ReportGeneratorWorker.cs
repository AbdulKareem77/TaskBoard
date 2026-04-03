using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskBoard.Infrastructure.Cache;
using TaskBoard.Infrastructure.Repositories;

namespace TaskBoard.NotificationConsumer.Workers;

public class ReportGeneratorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReportGeneratorWorker> _logger;
    private const string QueueName = "report-requests";

    public ReportGeneratorWorker(IServiceProvider serviceProvider, ILogger<ReportGeneratorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // yield immediately so host startup is not blocked

        _logger.LogInformation("ReportGeneratorWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessReportRequestAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing report request.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _logger.LogInformation("ReportGeneratorWorker stopped.");
    }

    private async Task ProcessReportRequestAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();
        var redis = scope.ServiceProvider.GetRequiredService<IInMemoryRedis>();
        var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
        var projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();

        var request = await queueService.DequeueAsync<ReportRequest>(QueueName, TimeSpan.FromSeconds(5));
        if (request == null)
            return;

        _logger.LogInformation("Generating report for project {ProjectId}, reportId={ReportId}.",
            request.ProjectId, request.ReportId);

        try
        {
            // Fetch project details
            var project = await projectRepository.GetByIdAsync(request.ProjectId);

            // Fetch tasks for the project
            var tasksResult = await taskRepository.GetByProjectIdAsync(request.ProjectId, 1, int.MaxValue, null);
            var tasks = tasksResult.Items.ToList();

            // Build report
            var report = new
            {
                ReportId = request.ReportId,
                GeneratedAt = DateTime.UtcNow,
                ProjectId = request.ProjectId,
                ProjectName = project?.Name ?? "Unknown",
                TotalTasks = tasks.Count,
                TasksByStatus = tasks
                    .GroupBy(t => t.Status)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TasksByPriority = tasks
                    .GroupBy(t => t.Priority ?? "None")
                    .ToDictionary(g => g.Key, g => g.Count()),
                OverdueTasks = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != "Done"),
                CompletedTasks = tasks.Count(t => t.Status == "Done")
            };

            var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            var reportKey = $"report:{request.ReportId}";

            // Store in InMemoryRedis with 10-minute TTL
            await redis.SetAsync(reportKey, reportJson, TimeSpan.FromMinutes(10));

            _logger.LogInformation("Report {ReportId} generated successfully.", request.ReportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report {ReportId}.", request.ReportId);

            // Store error result
            var errorReport = JsonSerializer.Serialize(new
            {
                ReportId = request.ReportId,
                Status = "Failed",
                Error = ex.Message
            });
            await redis.SetAsync($"report:{request.ReportId}", errorReport, TimeSpan.FromMinutes(5));
        }
    }

    private record ReportRequest(Guid ReportId, Guid ProjectId);
}
