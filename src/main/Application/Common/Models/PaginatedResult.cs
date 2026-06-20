namespace Application.Common.Models;

public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PaginatedResult<T> Create(IReadOnlyList<T> items, int total, int page, int pageSize) =>
        new() { Items = items, Total = total, Page = page, PageSize = pageSize };
}
