using Application.DTOs.CreateDTOs;
using Application.DTOs.ResponseDTOs;
using Application.DTOs.UpdateDTOs;
using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System.Security.Claims;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HabitController : ControllerBase
{
    private readonly IHabitService _habitService;

    public HabitController(IHabitService habitService) => _habitService = habitService;

    // GET: /api/habit/all
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ResponseHabitDto>>> GetAllHabits()
    {
        var result = await _habitService.GetAllHabitsAsync();
        return Ok(result);
    }

    // GET: /api/habit
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResponseHabitDto>>> GetHabitsByUserId()
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        var result = await _habitService.GetHabitsByUserIdAsync(userId.Value);

        return Ok(result);
    }

    // GET: /api/habit/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseHabitDto>> GetHabitById(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        try
        {
            var result = await _habitService.GetHabitByIdAsync(id, userId.Value);
            if (result == null)
                return NotFound(new { message = "Привычка не найдена" });

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // POST: /api/habit
    [HttpPost]
    public async Task<ActionResult<ResponseHabitDto>> CreateHabit(CreateHabitDto createHabit)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        createHabit.UserId = userId.Value;

        try
        {
            var result = await _habitService.CreateHabitAsync(createHabit);

            return CreatedAtAction(nameof(CreateHabit), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: /api/habit/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHabitAsync(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        try
        {
            var result = await _habitService.DeleteHabitAsync(id, userId.Value);
            if (!result)
                return NotFound(new { message = "Привычка не найдена" });

            return Ok(new { message = "Привычка успешна удалена" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // PUT: /api/habit/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ResponseHabitDto>> UpdateHabitAsync(int id, UpdateHabitDto updateHabit)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        try
        {
            var result = await _habitService.UpdateHabitAsync(id, userId.Value, updateHabit);
            if (result == null)
                return NotFound(new { message = "Привычка не найдена" });

            return Ok(new { message = "Привычка успешна обновлена" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;
        return userId;
    }
}