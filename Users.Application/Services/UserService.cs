using Users.Application.Interfaces.Identity;
using Users.Application.Interfaces.Repositories;
using Users.Application.Interfaces.Services;
using Users.Application.Models;
using Users.Domain.Enums;
using Users.Domain.Exceptions;
using Users.Domain.Users;
using Users.Domain.Users.Parameters;

namespace Users.Application.Services;

public sealed class UserService(
    IPasswordHasher passwordHasher,
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator) : IUserService
{
    public async Task CreateAsync(CreateUserRequest body, CancellationToken token)
    {
        var user = await userRepository.GetByLoginAsync(body.Login, token);

        if (!ReferenceEquals(user, null))
            throw new UserWithLoginIsAlreadyExistException();
        
        var passwordHash = passwordHasher.GetPasswordHash(body.Password);

        var newUser = User.Create(new CreateUserParameter
        {
            Login = body.Login,
            PasswordHash = passwordHash,
            Role = body.Role ?? UserRoleEnum.User
        });
        
        userRepository.Add(newUser);
        
        await userRepository.SaveChangesAsync(token);
    }

    public async Task<string> LoginAsync(LoginUserRequest body, CancellationToken token)
    {
        var user = await userRepository.GetByLoginAsync(body.Login, token);

        if (user is null)
            throw new KeyNotFoundException("Неверный логин или пароль.");

        var valid = passwordHasher.IsPasswordValid(
            body.Password,
            user.PasswordHash);

        if (!valid)
            throw new KeyNotFoundException("Неверный логин или пароль.");

        return jwtTokenGenerator.GenerateToken(user);
    }
}