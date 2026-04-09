namespace Domain.Models;

public class HabitLog
{
    public int Id { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int HabitId { get; set; }
    public Habit Habit { get; set; } = null!;
}