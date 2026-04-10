using Application.DTOs.CreateDTOs;
using Application.DTOs.ResponseDTOs;
using Application.DTOs.UpdateDTOs;

namespace Application.Interfaces;

public interface IHabitLogService
{
    Task<IEnumerable<ResponseHabitLogDto>> GetLogsByUserIdAsync(int userId);
    Task<IEnumerable<ResponseHabitLogDto>> GetLogsByHabitIdAsync(int habitId, int userId);
    Task<ResponseHabitLogDto?> GetLogByIdAsync(int id, int userId);
    Task<ResponseHabitLogDto> CreateLogAsync(CreateHabitLogDto createLog);
    Task<ResponseHabitLogDto?> UpdateLogAsync(int id, int userId, UpdateHabitLogDto updateLog);
    Task<bool> DeleteLogAsync(int id, int userId);
}