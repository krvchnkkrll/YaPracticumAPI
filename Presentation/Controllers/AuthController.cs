using Application.Interfaces.Services;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

public sealed class AuthController(IUserService userService) : AppController
{
    /// <summary>
    ///     Регистрация
    /// </summary>
    [HttpPost("/auth/register")]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateUserResponse>> RegisterAsync([FromBody] CreateUserRequest body, CancellationToken token)
    {
        return Ok(await userService.CreateAsync(body, token));
    }

    /// <summary>
    ///     Авторизация 
    /// </summary>
    [HttpPost("/auth/login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> LoginAsync([FromBody] LoginUserRequest body, CancellationToken token)
    {
        return Ok(await userService.LoginAsync(body, token));
    }
}