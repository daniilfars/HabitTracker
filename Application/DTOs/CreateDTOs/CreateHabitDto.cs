using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CreateDTOs;

public class CreateHabitDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public FrequencyType Frequency { get; set; }

    public List<DayOfWeek>? CustomDays { get; set; }

    public int UserId { get; set; }
}
