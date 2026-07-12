using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities.Users;
using Domain.Entities.Users.Parameters;
using Domain.Enums;

namespace Application.Services;

internal sealed class UserService(
    IPasswordHasher passwordHasher,
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator) : IUserService
{
    public async Task<CreateUserResponse> CreateAsync(CreateUserRequest body, CancellationToken token)
    {
        var passwordHash = passwordHasher.GetPasswordHash(body.Password);

        var user = User.Create(new CreateUserParameter
        {
            Login = body.Login,
            PasswordHash = passwordHash,
            Role = UserRoleEnum.User
        });
        
        userRepository.Add(user);
        
        await userRepository.SaveChangesAsync(token);

        return new CreateUserResponse
        {
            Login = body.Login
        };
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