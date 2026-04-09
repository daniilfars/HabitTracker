using Domain.Enums;

namespace Domain.Models;

public class Habit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FrequencyType Frequency { get; set; }
    public List<DayOfWeek>? CustomDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<HabitLog> HabitLogs { get; set; } = new();
}