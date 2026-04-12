using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs.Jwt;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterDto registerDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerDto);
            SetTokenCookies(result.Token, result.RefreshToken);
            return CreatedAtAction(nameof(Register), new { email = result.User?.Email}, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new {message = "Внутренняя ошибка сервера"});
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
            return Unauthorized(new {message = "Неверный email или пароль"});

        SetTokenCookies(result.Token, result.RefreshToken);

        return Ok(result.User);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token отсутствует" });

        try
        {
            var result = await _authService.RefreshTokenAsync(Request.Cookies["refreshToken"]);
            SetTokenCookies(result.Token, result.RefreshToken);
            return Ok(result.User);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(refreshToken);
        }

        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Успешный выход из системы" });
    }

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps, // Для тестов
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        Response.Cookies.Append("accessToken", accessToken, options);
        Response.Cookies.Append("refreshToken", refreshToken, options);
    }
}