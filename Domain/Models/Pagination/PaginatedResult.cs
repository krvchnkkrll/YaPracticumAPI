namespace Domain.Models.Pagination;

public sealed class PaginatedResult<T>
{
    /// <summary>
    ///     Элементы
    /// </summary>
    public required IReadOnlyCollection<T> Items { get; init; }
    
    /// <summary>
    ///     Общее количество элементов
    /// </summary>
    public required int TotalItems { get; init; }
    
    /// <summary>
    ///     Текущая страница
    /// </summary>
    public required int CurrentPage { get; init; }
    
    /// <summary>
    ///     Общее количество страниц
    /// </summary>
    public required int TotalPage { get; init; }
    
    /// <summary>
    ///     Есть ли предыдущая страница
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;
    
    /// <summary>
    ///     Есть ли следующая страница
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
    
    /// <summary>
    ///     Размер страницы
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    ///     Всего страниц при текущем размере страницы
    /// </summary>
    public required int TotalPages { get; init; }
}
