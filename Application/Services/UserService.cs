using Application.DTOs.CreateDTOs;
using Application.DTOs.ResponseDTOs;
using Application.DTOs.UpdateDTOs;
using Domain.Models;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserService : IUserService
{
    readonly IAppDbContext db;

    public UserService(IAppDbContext context) => db = context;

    public async Task<User> CreateUserAsync(CreateUserDto createUser)
    {
        User? existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == createUser.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Пользователь с таким email уже существует");

        User user = new User
        {
            Name = createUser.Name,
            Email = createUser.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUser.Password)
        };

        if (user.Email == "admin@example.com")
            user.Role = "Admin";

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ResponseUserDto>> GetAllUsersAsync()
    {
        return await db.Users.AsNoTracking().Select(u => new ResponseUserDto
        {
            CreatedAt = u.CreatedAt,
            Email = u.Email,
            Id = u.Id,
            Name = u.Name,
            UpdatedAt = u.UpdatedAt
        }).ToListAsync();
    }

    public async Task<ResponseUserDto?> GetUserByIdAsync(int id)
    {
        User? user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return null;

        return new ResponseUserDto
        {
            CreatedAt = user.CreatedAt,
            Email = user.Email,
            Id = user.Id,
            Name = user.Name,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<ResponseUserDto?> UpdateUserAsync(int id, UpdateUserDto updateUser)
    {
        User? user = await db.Users.FindAsync(id);
        if (user == null)
            return null;

        if (!string.IsNullOrWhiteSpace(updateUser.Email))
        {
            User? existingUser = await db.Users.FirstOrDefaultAsync(u => u.Email == updateUser.Email && u.Id != user.Id);
            if (existingUser != null)
                throw new InvalidOperationException("Пользователь с таким email уже существует");

            user.Email = updateUser.Email;
        }

        if (!string.IsNullOrWhiteSpace(updateUser.Name))
            user.Name = updateUser.Name;

        if (!string.IsNullOrWhiteSpace(updateUser.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUser.Password);

        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return new ResponseUserDto
        {
            CreatedAt = user.CreatedAt,
            Email = user.Email,
            Id = user.Id,
            Name = user.Name,
            UpdatedAt = DateTime.UtcNow
        };
    }
}