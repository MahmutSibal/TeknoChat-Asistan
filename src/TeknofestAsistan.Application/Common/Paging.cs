namespace TeknofestAsistan.Application.Common;

public static class Paging
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public static (int PageNumber, int PageSize) Normalize(int pageNumber, int pageSize) =>
        (Math.Max(pageNumber, 1), Math.Clamp(pageSize <= 0 ? DefaultPageSize : pageSize, 1, MaxPageSize));
}
