#nullable enable

using Unity.VisualScripting;

public class PaginationQueryDto
{
    public static PaginationQueryDto Default => new PaginationQueryDto();

    public PaginationQueryDto()
    {
        PageSize = 15;
        ContinuationToken = null;
    }

    public PaginationQueryDto(int pageSize, string? continuationToken = null)
    {
        PageSize = pageSize;
        ContinuationToken = continuationToken;
    }

    public int PageSize { get; set; } = 1;
    public string? ContinuationToken { get; set; }
}