using Application.DTOs.Calendar;
using Application.Interfaces;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class CalendarService : ICalendarService
{
    private readonly IAppDbContext _context;

    public CalendarService(IAppDbContext context) => _context = context;

    public async Task<CalendarResponseDto> GetUserCalendarAsync(int userId, int daysBack = 365)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var cutoffDate = today.AddDays(-daysBack);

        // Получаем все привычки пользователя, созданные до сегодняшнего дня(today)
        var habits = await _context.Habits
            .AsNoTracking()
            .Where(h => h.UserId == userId && DateOnly.FromDateTime(h.CreatedAt.Date) <= today)
            .ToListAsync();

        // Агрегируем логи на стороне БД
        var logCounts = await _context.HabitLogs
            .AsNoTracking()
            .Where(l => l.Habit.UserId == userId && l.Date >= cutoffDate && l.IsCompleted)
            .GroupBy(l => l.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.Date, v => v.Count);

        var daysList = new List<CalendarDayDto>();
        int totalCompleted = 0;

        // Проходим по каждой дате в диапазоне
        for (var date = cutoffDate; date <= today; date = date.AddDays(1))
        {
            // Считаем, сколько привычек должны быть выполнены в этот день
            int expectedCount = habits.Count(h =>
                DateOnly.FromDateTime(h.CreatedAt.Date) <= date &&
                IsTargetDay(h, date));

            logCounts.TryGetValue(date, out int completed);

            var dayDto = new CalendarDayDto
            {
                Date = date,
                CompletedCount = completed,
                TotalCount = expectedCount,
                IsPerfectDay = expectedCount > 0 && completed >= expectedCount
            };

            daysList.Add(dayDto);
            totalCompleted += completed;
        }

        // Находим лучший день
        var bestDay = daysList.MaxBy(d => d.CompletedCount);

        return new CalendarResponseDto
        {
            Days = daysList,
            TotalCompleted = totalCompleted,
            BestDay = bestDay,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<UserStreakDto> GetUserStreakAsync(int userId)
    {
        // Фиксируем текущую дату в формате DateOnly для исключения влияния времени (часов/минут)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 1. ЗАГРУЗКА ДАННЫХ
        // Берем только активные привычки. Если привычка удалена/архивирована, она не участвует в расчете текущей серии.
        var habits = await _context.Habits
            .AsNoTracking()
            .Where(h => h.UserId == userId && h.IsActive)
            .ToListAsync();

        // Если у пользователя нет активных привычек, то и серий быть не может
        if (!habits.Any()) return new UserStreakDto { CurrentStreak = 0, BestStreak = 0 };

        // Загружаем только успешные отметки (IsCompleted). 
        // Нам важны только даты и ID привычек для сопоставления с планом.
        var completedLogs = await _context.HabitLogs
            .AsNoTracking()
            .Where(l => l.Habit.UserId == userId && l.IsCompleted)
            .Select(l => new { l.Date, l.HabitId })
            .ToListAsync();

        // Превращаем список логов в быстрый справочник (Lookup):
        // Ключ: Дата, Значение: Набор (HashSet) ID привычек, выполненных в этот день.
        // HashSet позволяет проверять наличие ID за O(1), что критично внутри цикла.
        var logsLookup = completedLogs
            .GroupBy(l => l.Date)
            .ToDictionary(g => g.Key, g => g.Select(x => x.HabitId).ToHashSet());

        // Точка начала анализа — дата создания самой старой привычки пользователя
        var startDate = habits.Min(h => DateOnly.FromDateTime(h.CreatedAt));

        int bestStreak = 0;    // Максимальная серия за всё время
        int currentStreak = 0; // Накопитель текущей серии в цикле

        // 2. РАСЧЕТ СЕРИИ (Эмуляция течения времени день за днем)
        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            // Вычисляем "План на день": какие из активных привычек должны были быть выполнены сегодня?
            // Привычка учитывается, если она уже была создана к этой дате и день является целевым (IsTargetDay)
            var targetHabitIds = habits
                .Where(h => DateOnly.FromDateTime(h.CreatedAt) <= date && IsTargetDay(h, date))
                .Select(h => h.Id)
                .ToList();

            // ЛОГИКА "ЗАМОРОЗКИ": Если на сегодня задач нет (выходной), мы просто пропускаем день.
            // Серия не прерывается и не растет — она "замораживается" до следующего рабочего дня.
            if (targetHabitIds.Count == 0) continue;

            // "Факт на день": смотрим, какие привычки из логов совпадают с планом
            logsLookup.TryGetValue(date, out var completedIds);

            // День считается успешным только если ВЫПОЛНЕНЫ ВСЕ запланированные на сегодня привычки
            bool allDone = targetHabitIds.All(id => completedIds != null && completedIds.Contains(id));

            if (allDone)
            {
                currentStreak++;
                // Если текущая серия побила рекорд — обновляем лучший результат
                if (currentStreak > bestStreak) bestStreak = currentStreak;
            }
            else
            {
                // ЛОГИКА СБРОСА:
                // Если сегодня (date == today) пользователь еще не выполнил все задачи — это НЕ повод обнулять серию.
                // У него есть время до конца дня. В этом случае мы просто оставляем стрейк "вчерашним".
                // А если пропуск был в любой день ДО сегодня (date < today) — серия обнуляется окончательно.
                if (date < today)
                {
                    currentStreak = 0;
                }
            }
        }

        return new UserStreakDto
        {
            CurrentStreak = currentStreak,
            BestStreak = bestStreak
        };
    }

    private static bool IsTargetDay(Habit habit, DateOnly date)
    {
        return habit.Frequency switch
        {
            FrequencyType.Daily => true,
            FrequencyType.Weekly => date.DayOfWeek == habit.CreatedAt.DayOfWeek,
            FrequencyType.Custom => habit.CustomDays?.Contains(date.DayOfWeek) == true,
            _ => false
        };
    }
}
