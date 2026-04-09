using Domain.Enums;

namespace Application.DTOs.ResponseDTOs;

public class ResponseHabitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FrequencyType Frequency { get; set; }
    public List<DayOfWeek>? CustomDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
}