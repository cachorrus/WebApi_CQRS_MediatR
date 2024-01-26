namespace MediatRApi.ApplicationCore.Common.Models;

public class PagedResult<T> where T : class
{
    public IEnumerable<T> Results { get; set; } = [];
    public int RowsCount { get; set; }
    public int PageCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
}