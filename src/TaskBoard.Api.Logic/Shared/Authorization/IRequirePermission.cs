namespace TaskBoard.Api.Logic.Shared.Authorization;

public interface IRequirePermission
{
    string Permission { get; }
}
