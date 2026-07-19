namespace Domain.Enums;

public enum BookingStatus
{
    /// <summary>
    ///     Бронь создана
    /// </summary>
    Pending = 1,
    
    /// <summary>
    ///     Бронь подтверждена
    /// </summary>
    Confirmed = 2,
    
    /// <summary>
    ///     Бронь отклонена
    /// </summary>
    Rejected = 3,
    
    /// <summary>
    ///     Брноь отменана
    /// </summary>
    Cancelled = 4
}