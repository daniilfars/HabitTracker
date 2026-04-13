namespace Application.DTOs.Calendar;

public class CalendarDayDto
{
    public DateOnly Date { get; set; }
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
    public bool IsPerfectDay { get; set; }
}
