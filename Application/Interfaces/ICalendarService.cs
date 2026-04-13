using Application.DTOs.Calendar;

namespace Application.Interfaces;

public interface ICalendarService
{
    Task<CalendarResponseDto> GetUserCalendarAsync(int userId, int daysBack = 365);
    Task<UserStreakDto> GetUserStreakAsync(int userId);
}
