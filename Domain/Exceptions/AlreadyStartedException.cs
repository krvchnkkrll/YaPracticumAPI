namespace Domain.Exceptions;

public sealed class EventAlreadyStartedException() : Exception("Бронирование недоступно: событие уже началось.");