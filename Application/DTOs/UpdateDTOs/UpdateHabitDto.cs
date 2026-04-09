using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.UpdateDTOs;

public class UpdateHabitDto
{
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Description { get; set; }
    public FrequencyType Frequency { get; set; }
    public List<DayOfWeek>? CustomDays { get; set; }
    public bool IsActive { get; set; }
}