using Application.DTOs.CreateDTOs;
using Application.DTOs.ResponseDTOs;
using Application.DTOs.UpdateDTOs;

namespace Application.Interfaces;

public interface IHabitLogService
{
    Task<ResponseHabitLogDto?> GetLogsByUserIdAsync(int userId);
    Task<IEnumerable<ResponseHabitLogDto>> GetLogsByHabitIdAsync(int habitId);
    Task<ResponseHabitLogDto?> GetLogByIdAsync(int id);
    Task<ResponseHabitLogDto> CreateLogAsync(CreateHabitLogDto createLog);
    Task<ResponseHabitLogDto?> UpdateLogAsync(int id, UpdateHabitLogDto updateLog);
    Task<bool> DeleteLogAsync(int id);
}