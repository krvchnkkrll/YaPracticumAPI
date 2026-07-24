namespace Users.Domain.Exceptions;

public sealed class UserWithLoginIsAlreadyExistException() : Exception("Пользователь с таким логином уже есть.");