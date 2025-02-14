#nullable enable

public class PaginationQueryDto
{
    public PaginationQueryDto(int pageSize, string? continuationToken = null)
    {
        PageSize = pageSize;
        ContinuationToken = continuationToken;
    }

    public int PageSize { get; set; } = 1;
    public string? ContinuationToken { get; set; }
}