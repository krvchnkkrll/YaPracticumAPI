using Application.Models;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<CreateUserResponse> CreateAsync(CreateUserRequest body, CancellationToken token);
    
    Task<string> LoginAsync(LoginUserRequest body, CancellationToken token);
}