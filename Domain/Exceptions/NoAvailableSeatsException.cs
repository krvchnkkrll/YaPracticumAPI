namespace Domain.Exceptions;

public sealed class NoAvailableSeatsException() : Exception("На выбранное событие больше нет мест"){}