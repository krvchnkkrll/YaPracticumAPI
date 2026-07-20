namespace Bookings.Domain.Exceptions;

public sealed class BookingLimitExceededException(int limit) : Exception($"Превышен лимит активных броней: не более {limit} на пользователя.");