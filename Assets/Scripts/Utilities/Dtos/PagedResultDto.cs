#nullable enable
using System.Collections.Generic;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int CurrentPageCount { get; set; }
    public int PageSize { get; set; }
    public string? ContinuationToken { get; set; }
    public bool HasNextPage => !string.IsNullOrEmpty(ContinuationToken);
}