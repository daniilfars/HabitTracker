using Application.DTOs.CreateDTOs;
using Application.DTOs.ResponseDTOs;
using Application.DTOs.UpdateDTOs;
using Domain.Models;
using Domain.Enums;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class HabitService : IHabitService
{
    private readonly IAppDbContext db;

    public HabitService(IAppDbContext context) => db = context;

    public async Task<ResponseHabitDto> CreateHabitAsync(CreateHabitDto createHabit)
    {
        if (createHabit.Frequency == FrequencyType.Custom && (createHabit.CustomDays == null || createHabit.CustomDays.Count == 0))
        {
            throw new InvalidOperationException("Для частоты 'Custom' необходимо указать хотя бы один день недели");
        }

        if (createHabit.Frequency != FrequencyType.Custom && createHabit.CustomDays != null)
        {
            throw new InvalidOperationException("Поле CustomDays должно быть пустым для выбранной частоты");
        }

        var habit = new Habit
        {
            Name = createHabit.Name,
            Description = createHabit.Description,
            Frequency = createHabit.Frequency,
            CustomDays = createHabit.CustomDays,
            UserId = createHabit.UserId,
        };

        db.Habits.Add(habit);
        await db.SaveChangesAsync();

        return MapToResponse(habit);
    }

    public async Task<bool> DeleteHabitAsync(int id, int userId)
    {
        var habit = await db.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit == null)
            return false;

        if (habit.UserId != userId)
            throw new UnauthorizedAccessException("У вас нет прав на удаление этой привычки");

        db.Habits.Remove(habit);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<ResponseHabitDto>> GetAllHabitsAsync()
    {
        return await db.Habits.AsNoTracking().Select(h => MapToResponse(h)).ToListAsync();
    }

    public async Task<ResponseHabitDto?> GetHabitByIdAsync(int id, int userId)
    {
        var habit = await db.Habits.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
        if (habit == null)
            return null;

        if (habit.UserId != userId)
            throw new UnauthorizedAccessException("У вас нет прав на получение этой привычки");

        return MapToResponse(habit);
    }

    public async Task<IEnumerable<ResponseHabitDto>> GetHabitsByUserIdAsync(int userId)
    {
        return await db.Habits
        .AsNoTracking()
        .Where(h => h.UserId == userId)
        .OrderBy(h => h.Name)
        .ThenByDescending(h => h.CreatedAt)
        .Select(h => MapToResponse(h))
        .ToListAsync();
    }

    public async Task<ResponseHabitDto?> UpdateHabitAsync(int id, int userId, UpdateHabitDto updateHabit)
    {
        var habit = await db.Habits.FindAsync(id);
        if(habit == null)
            return null;

        if (habit.UserId != userId)
            throw new UnauthorizedAccessException("У вас нет прав на изменение этой привычки");

        if (!string.IsNullOrWhiteSpace(updateHabit.Name))
            habit.Name = updateHabit.Name;

        if (!string.IsNullOrWhiteSpace(updateHabit.Description))
            habit.Description = updateHabit.Description;

        if (updateHabit.Frequency != null)
            habit.Frequency = updateHabit.Frequency;

        if (updateHabit.CustomDays != null)
            habit.CustomDays = updateHabit.CustomDays;

        if (updateHabit.IsActive != null)
            habit.IsActive = updateHabit.IsActive;

        if (habit.Frequency == FrequencyType.Custom && (habit.CustomDays == null || habit.CustomDays.Count == 0))
        {
            throw new InvalidOperationException("Для частоты 'Custom' необходимо указать хотя бы один день недели");
        }

        habit.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return MapToResponse(habit);
    }

    private static ResponseHabitDto MapToResponse(Habit habit) => new()
    {
        Id = habit.Id,
        Name = habit.Name,
        Description = habit.Description,
        Frequency = habit.Frequency,
        CustomDays = habit.CustomDays,
        IsActive = habit.IsActive,
        CreatedAt = habit.CreatedAt,
        UpdatedAt = habit.UpdatedAt,
        UserId = habit.UserId
    };
}