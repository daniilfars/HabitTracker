using Application.DTOs.CreateDTOs;
using Application.DTOs.Jwt;
using Application.DTOs.ResponseDTOs;
using Application.Interfaces;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;

    public AuthService(IAppDbContext context, ITokenService tokenService, IUserService userService)
    {
        _context = context;
        _tokenService = tokenService;
        _userService = userService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            User? user = await _userService.CreateUserAsync(new CreateUserDto
            {
                Email = registerDto.Email,
                Name = registerDto.UserName,
                Password = registerDto.Password
            });

            var defaultHabit = new Habit
            {
                Name = $"Заправить постель",
                Description = "Привычка по умолчанию",
                UserId = user.Id,
                Frequency = FrequencyType.Daily
            };

            _context.Habits.Add(defaultHabit);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            string token = _tokenService.GenerateToken(user);
            string refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = new ResponseUserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<AuthResponse?> LoginAsync(LoginDto loginDto)
    {
        User? user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
            return null;

        bool passwordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
        if (!passwordValid)
            return null;

        string token = _tokenService.GenerateToken(user);
        string refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            User = new ResponseUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Недействительный или истекший refresh token");

        var newAccessToken = _tokenService.GenerateToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            User = new ResponseUserDto
            {
                Id = user.Id,
                Name = user.Name,
                CreatedAt = user.CreatedAt,
                Email = user.Email
            }
        };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user == null)
            return;

        user.RefreshToken = string.Empty;
        user.RefreshTokenExpiryTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}