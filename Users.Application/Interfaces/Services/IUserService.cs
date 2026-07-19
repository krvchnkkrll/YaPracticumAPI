using Users.Application.Models;

namespace Users.Application.Interfaces.Services;

public interface IUserService
{
    Task CreateAsync(CreateUserRequest body, CancellationToken token);
    
    Task<string> LoginAsync(LoginUserRequest body, CancellationToken token);
}