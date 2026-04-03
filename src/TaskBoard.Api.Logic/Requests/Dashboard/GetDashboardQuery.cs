using MediatR;
using TaskBoard.Api.Logic.Models;

namespace TaskBoard.Api.Logic.Requests.Dashboard;

public class GetDashboardQuery : IRequest<DashboardDto>
{
}
