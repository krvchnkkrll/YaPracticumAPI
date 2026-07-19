using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities.Users;
using Domain.Entities.Users.Parameters;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services;

internal sealed class UserService(
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