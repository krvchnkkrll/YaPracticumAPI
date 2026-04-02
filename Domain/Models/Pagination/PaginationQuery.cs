namespace Domain.Models.Pagination;

public sealed class PaginationQuery
{
    /// <summary>
    ///     Номер страницы (с единицы)
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    ///     Размер страницы (число элементов)
    /// </summary>
    public required int PageSize { get; init; }
}
