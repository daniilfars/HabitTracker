using Application.DTOs.CreateDTOs;
using Application.DTOs.ResponseDTOs;
using Application.DTOs.UpdateDTOs;
using Domain.Models;
using Domain.Enums;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class HabitLogService : IHabitLogService
{
    private readonly IAppDbContext db;

    public HabitLogService(IAppDbContext context) => db = context;

    public async Task<ResponseHabitLogDto> CreateLogAsync(CreateHabitLogDto createLog)
    {
        var habit = await db.Habits.FirstOrDefaultAsync(h => h.Id == createLog.HabitId);
        if(habit == null)
            throw new InvalidOperationException("Привычка не найдена");

        if (habit.UserId != createLog.UserId)
            throw new UnauthorizedAccessException("У вас нет прав на добавление записи для этой привычки");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existingLog = await db.HabitLogs.FirstOrDefaultAsync(l => l.HabitId == createLog.HabitId && l.Date == today);

        if (existingLog != null)
            throw new InvalidOperationException("Запись за сегодня уже существует");

        var log = new HabitLog
        {
            HabitId = createLog.HabitId,
            Date = today,
            IsCompleted = createLog.IsCompleted,
            Notes = createLog.Notes,
        };

        db.HabitLogs.Add(log);
        await db.SaveChangesAsync();

        return MapToResponse(log);
    }

    public async Task<bool> DeleteLogAsync(int id, int userId)
    {
        var log = await db.HabitLogs.Include(l => l.Habit).FirstOrDefaultAsync(l => l.Id == id);
        if (log == null)
            return false;

        if(log.Habit.UserId != userId)
            throw new UnauthorizedAccessException("У вас нет прав на удаление этой записи");

        db.HabitLogs.Remove(log);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<ResponseHabitLogDto?> GetLogByIdAsync(int id, int userId)
    {
        var log = await db.HabitLogs.AsNoTracking().Include(l => l.Habit).FirstOrDefaultAsync(l => l.Id == id);
        if (log == null)
            return null;

        if (log.Habit.UserId != userId)
            throw new UnauthorizedAccessException("У вас нет прав на просмотр этой записи");

        return MapToResponse(log);
    }

    public async Task<IEnumerable<ResponseHabitLogDto>> GetLogsByHabitIdAsync(int habitId, int userId)
    {
        var habit = await db.Habits.AsNoTracking().FirstOrDefaultAsync(h => h.Id == habitId);
        if (habit == null)
            throw new InvalidOperationException("Привычка не найдена");

        if (habit.Id != userId)
            throw new UnauthorizedAccessException("У вас нет прав на просмотр этой записи");

        return await db.HabitLogs
            .AsNoTracking()
            .Where(l => l.HabitId == habitId)
            .OrderByDescending(l => l.Date)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<IEnumerable<ResponseHabitLogDto>> GetLogsByUserIdAsync(int userId)
    {
        return await db.HabitLogs
            .AsNoTracking()
            .Include(l => l.Habit)
            .Where(l => l.Habit.UserId == userId)
            .OrderByDescending(l => l.Date)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<ResponseHabitLogDto?> UpdateLogAsync(int id, int userId, UpdateHabitLogDto updateLog)
    {
        var log = await db.HabitLogs.Include(l => l.Habit).FirstOrDefaultAsync(l => l.Id == id);
        if (log == null)
            return null;

        if (log.Habit.UserId != userId)
            throw new UnauthorizedAccessException("У вас нет прав на изменение этой записи");

        if (updateLog.IsCompleted.HasValue)
            log.IsCompleted = updateLog.IsCompleted.Value;

        if (updateLog.Notes != null)
            log.Notes = updateLog.Notes;

        await db.SaveChangesAsync();

        return MapToResponse(log);
    }

    private static ResponseHabitLogDto MapToResponse(HabitLog log) => new()
    {
        Id = log.Id,
        HabitId = log.HabitId,
        Date = log.Date,
        IsCompleted = log.IsCompleted,
        Notes = log.Notes,
        CreatedAt = log.CreatedAt
    };
}