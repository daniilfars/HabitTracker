using Domain.Models;
using Application.DTOs.CreateDTOs;
using Application.DTOs.UpdateDTOs;
using Application.DTOs.ResponseDTOs;

namespace Application.Interfaces;

public interface IHabitService
{
    Task<IEnumerable<ResponseHabitDto>> GetAllHabitsAsync();
    Task<IEnumerable<ResponseHabitDto>> GetHabitsByUserIdAsync(int userId);
    Task<ResponseHabitDto?> GetHabitByIdAsync(int id, int userId);
    Task<Habit> CreateHabitAsync(CreateHabitDto createHabit);
    Task<ResponseHabitDto?> UpdateHabitAsync(int id, int userId, UpdateHabitDto updateHabit);
    Task<bool> DeleteHabitAsync(int id, int userId);
}