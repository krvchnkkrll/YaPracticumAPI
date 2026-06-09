namespace Application.Contracts.Models;

public class GetBookingResponse
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Внешний ключ события
    /// </summary>
    public required Guid EventId { get; init; }
    
    /// <summary>
    ///     Добавлена в
    /// </summary>
    public required DateTime CreatedAt { get; init; }
    
    /// <summary>
    ///     Обработана в
    /// </summary>
    public required DateTime? ProcessedAt { get; init; }
    
    /// <summary>
    ///     Статус брони
    /// </summary>
    public required string Status { get; init; }
}