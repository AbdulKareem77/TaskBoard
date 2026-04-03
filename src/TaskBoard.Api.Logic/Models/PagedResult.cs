namespace TaskBoard.Api.Logic.Models;

public record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
