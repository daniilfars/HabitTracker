using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CreateDTOs;

public class CreateHabitLogDto
{
    [Required]
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public int HabitId { get; set; }
    public int UserId { get; set; }
}
