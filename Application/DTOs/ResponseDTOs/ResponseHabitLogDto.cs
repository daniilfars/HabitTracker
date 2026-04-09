namespace Application.DTOs.ResponseDTOs;

public class ResponseHabitLogDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int HabitId { get; set; }
}