namespace Giglio.EduCore.Financial.Application.DTOs;

public record PagedResult<T>
{
    public List<T> Data { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
