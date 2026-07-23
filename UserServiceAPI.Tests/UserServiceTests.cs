using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Interfaces.Identity;
using Users.Application.Interfaces.Repositories;
using Users.Application.Interfaces.Services;
using Users.Application.Models;
using Users.Application.Services;
using Users.Domain.Enums;
using Users.Domain.Exceptions;
using Users.Infrastructure;
using Users.Infrastructure.Identity;
using Users.Infrastructure.Interfaces;
using Users.Infrastructure.Models;
using Users.Infrastructure.Repositories;

namespace UserServiceAPI.Tests;

public sealed class UserServiceTests
{
    private readonly ServiceProvider _serviceProvider;

    private readonly CreateUserRequest _createRequest = new CreateUserRequest
    {
        Login = "TestUser",
        Password = "TestUser",
        Role = UserRoleEnum.User
    };
    
    private readonly LoginUserRequest _loginRequest = new LoginUserRequest
    {
        Login = "TestUser",
        Password = "TestUser"
    };
    
    public UserServiceTests()
    {
        var dbName = Guid.CreateVersion7().ToString();

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.Configure<JwtOptions>(options =>
        {
            options.Secret = "Q53LoLDRd6lEB3EZGF/1O3R08YC/VvEoqKiTANzSL9Q=";
            options.Issuer = "BookingApi";
            options.Audience = "BookingApiClient";
            options.Lifetime = 300;
        });

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Create_NewUser_ReturnNoException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();

        await FluentActions
            .Invoking(() => service.CreateAsync(_createRequest, CancellationToken.None))
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task Create_NewUser_ThrowUserWithLoginIsAlreadyExistException()
    {
        await SeedUserAsync();
        
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();
        
        await FluentActions
            .Invoking(() => service.CreateAsync(_createRequest, CancellationToken.None))
            .Should()
            .ThrowAsync<UserWithLoginIsAlreadyExistException>();
    }

    [Fact]
    public async Task Login_User_ThrowKeyNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();

        await FluentActions
            .Invoking(() => service.LoginAsync(_loginRequest, CancellationToken.None))
            .Should()
            .ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task Login_User_ReturnJwtToken()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();
        
        await SeedUserAsync();
        
        await FluentActions
            .Invoking(() => service.LoginAsync(_loginRequest, CancellationToken.None))
            .Should()
            .NotThrowAsync();
    }

    private async Task SeedUserAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserService>();

        await service.CreateAsync(_createRequest, CancellationToken.None);
    }
}