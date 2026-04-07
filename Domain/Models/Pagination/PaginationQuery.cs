namespace Domain.Models.Pagination;

public sealed class PaginationQuery
{
    /// <summary>
    ///     Номер страницы (с единицы). Если параметр не передан или ≤ 0, используется 1.
    /// </summary>
    public int Page { get; set; } 

    /// <summary>
    ///     Размер страницы (число элементов). Если параметр не передан или ≤ 0, используется 10.
    /// </summary>
    public int PageSize { get; set; }
}
