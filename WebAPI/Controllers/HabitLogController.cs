using Application.DTOs.CreateDTOs;
using Application.DTOs.ResponseDTOs;
using Application.DTOs.UpdateDTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers;

[Route("api/log")]
[ApiController]
public class HabitLogController : ControllerBase
{
    private readonly IHabitLogService _habitLog;

    public HabitLogController(IHabitLogService habitLog) => _habitLog = habitLog;

    // GET: /api/log/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ResponseHabitLogDto>> GetLogById(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        try
        {
            var result = await _habitLog.GetLogByIdAsync(id, userId.Value);
            if (result == null)
                return NotFound(new { message = "Запись о выполнении привычки не найдена" });

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // GET: /api/log/habit/5
    [HttpGet("habit/{id}")]
    public async Task<ActionResult<IEnumerable<ResponseHabitLogDto>>> GetLogsByHabitId(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        try
        {
            var result = await _habitLog.GetLogsByHabitIdAsync(id, userId.Value);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET: /api/log/user
    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<ResponseHabitLogDto>>> GetLogsByUserId()
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        var result = await _habitLog.GetLogsByUserIdAsync(userId.Value);
        return Ok(result);
    }

    // POST: /api/log
    [HttpPost]
    public async Task<ActionResult<ResponseHabitLogDto>> CreateLog(CreateHabitLogDto createLog)
    {
        var id = GetUserIdFromToken();
        if (id == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        createLog.UserId = id.Value;

        try
        {
            var result = await _habitLog.CreateLogAsync(createLog);
            return CreatedAtAction(nameof(CreateLog) , new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // DELETE: /api/log/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLogAsync(int id)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        try
        {
            var result = await _habitLog.DeleteLogAsync(id, userId.Value);
            if(!result)
                return NotFound(new { message = "Запись не найдена" });

            return Ok(new { message = "Запись успешна удалена" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // PUT: /api/log/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ResponseHabitLogDto>> UpdateLogAsync(int id, UpdateHabitLogDto updateLog)
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        try
        {
            var result = await _habitLog.UpdateLogAsync(id, userId.Value, updateLog);
            if (result == null)
                return NotFound(new { message = "Запись не найдена" });

            return Ok(new { message = "Запись успешна обновлена" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
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