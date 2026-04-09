using Domain.Models;
using Application.DTOs.CreateDTOs;
using Application.DTOs.UpdateDTOs;
using Application.DTOs.ResponseDTOs;

namespace Application.Interfaces;

public interface IHabitService
{
    Task<IEnumerable<ResponseHabitDto>> GetAllHabitsAsync();
    Task<ResponseHabitDto?> GetHabitByIdAsync(int id);
    Task<Habit> CreateHabitAsync(CreateHabitDto createHabit);
    Task<ResponseHabitDto?> UpdateHabitAsync(int id, UpdateHabitDto updateHabit);
    Task<bool> DeleteHabitAsync(int id);
}