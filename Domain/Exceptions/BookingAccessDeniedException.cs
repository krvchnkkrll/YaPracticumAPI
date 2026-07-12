namespace Domain.Exceptions;

public sealed class BookingAccessDeniedException() : Exception($"Пользователь не может изменять эту бронь.");