using FluentAssertions;
using Users.Domain.Enums;
using Users.Domain.Users;
using Users.Domain.Users.Parameters;
using UserServiceAPI.IntegrationTests.Fixtures;

namespace UserServiceAPI.IntegrationTests;

public class UserRepositoryTest : IntegrationTestBase
{
    private readonly CreateUserParameter _createUserParameter = new CreateUserParameter
    {
        Login = "TestUser",
        PasswordHash = "RandomHashedPassword",
        Role = UserRoleEnum.User
    };
    
    [Fact]
    public async Task GetByLogin_ExistUser_ReturnsValidUser()
    {
        var userEntity = await CreateAndSaveUserAsync();
        
        var user = await UserRepository.GetByLoginAsync(_createUserParameter.Login, CancellationToken.None);
        
        user.Should().NotBeNull();
        user.Login.Should().Be(userEntity.Login);
        user.PasswordHash.Should().Be(userEntity.PasswordHash);
        user.Role.Should().Be(userEntity.Role);
    }
    
    [Fact]
    public async Task GetByLogin_NotExistUser_ThrowKeyNotFoundException()
    {
        var user = await UserRepository.GetByLoginAsync(_createUserParameter.Login, CancellationToken.None);
        
        user.Should().BeNull();
    }

    private async Task<User> CreateAndSaveUserAsync()
    {
        var userEntity = User.Create(_createUserParameter);
        UserRepository.Add(userEntity);
        await UserRepository.SaveChangesAsync(CancellationToken.None);
        return userEntity;
    }
}