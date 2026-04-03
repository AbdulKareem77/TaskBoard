using MediatR;

namespace TaskBoard.Api.Logic.Requests.Reports;

public record ReportQueuedDto(Guid ReportId, string Status);

public class RequestReportCommand : IRequest<ReportQueuedDto>
{
    public Guid ProjectId { get; init; }
}
