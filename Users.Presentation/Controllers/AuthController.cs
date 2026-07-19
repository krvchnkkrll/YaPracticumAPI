using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Interfaces.Services;
using Users.Application.Models;

namespace Users.Presentation.Controllers;

public sealed class AuthController(IUserService userService) : AppController
{
    /// <summary>
    ///     Регистрация
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/auth/register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RegisterAsync([FromBody] CreateUserRequest body, CancellationToken token)
    {
        await userService.CreateAsync(body, token);
        return NoContent();
    }

    /// <summary>
    ///     Авторизация 
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/auth/login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> LoginAsync([FromBody] LoginUserRequest body, CancellationToken token)
    {
        return Ok(await userService.LoginAsync(body, token));
    }
}