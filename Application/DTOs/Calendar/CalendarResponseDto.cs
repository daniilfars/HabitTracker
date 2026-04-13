namespace Application.DTOs.Calendar;

public class CalendarResponseDto
{
    public List<CalendarDayDto> Days { get; set; }
    public int TotalCompleted { get; set; }
    public CalendarDayDto? BestDay { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
