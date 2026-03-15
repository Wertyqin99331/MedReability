namespace MedReability.Application.DTOs.Common;

public class PagedResultDto<TItem>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<TItem> Items { get; set; } = [];
}
