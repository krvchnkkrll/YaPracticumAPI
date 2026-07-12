using Domain.Entities.Users;

namespace Application.Interfaces.Identity;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}