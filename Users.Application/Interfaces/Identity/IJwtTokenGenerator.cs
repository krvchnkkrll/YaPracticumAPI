using Users.Domain.Users;

namespace Users.Application.Interfaces.Identity;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}