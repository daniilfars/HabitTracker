using Domain.Models;
using Application.DTOs.CreateDTOs;
using Application.DTOs.UpdateDTOs;
using Application.DTOs.ResponseDTOs;

namespace Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<ResponseUserDto>> GetAllUsersAsync();
    Task<ResponseUserDto?> GetUserByIdAsync(int id);
    Task<ResponseUserDto> CreateUserAsync(CreateUserDto createUser);
    Task<ResponseUserDto?> UpdateUserAsync(int id, UpdateUserDto updateUser);
    Task<bool> DeleteUserAsync(int id);
}