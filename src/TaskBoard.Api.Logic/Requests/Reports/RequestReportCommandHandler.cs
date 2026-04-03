using MediatR;
using TaskBoard.Infrastructure.Cache;

namespace TaskBoard.Api.Logic.Requests.Reports;

public class RequestReportCommandHandler : IRequestHandler<RequestReportCommand, ReportQueuedDto>
{
    private readonly IQueueService _queueService;

    public RequestReportCommandHandler(IQueueService queueService)
    {
        _queueService = queueService;
    }

    public async Task<ReportQueuedDto> Handle(RequestReportCommand request, CancellationToken cancellationToken)
    {
        var reportId = Guid.NewGuid();

        await _queueService.EnqueueAsync("report-requests", new
        {
            ReportId = reportId,
            ProjectId = request.ProjectId
        });

        return new ReportQueuedDto(reportId, "Queued");
    }
}
