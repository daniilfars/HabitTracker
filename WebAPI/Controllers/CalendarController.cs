using Application.DTOs.Calendar;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService) => _calendarService = calendarService;

    // GET: /api/calendar
    [HttpGet]
    public async Task<ActionResult<CalendarResponseDto>> GetUserCalendarAsync()
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        var result = await _calendarService.GetUserCalendarAsync(userId.Value);
        return Ok(result);
    }

    // GET: /api/calendar/streak
    [HttpGet("streak")]
    public async Task<ActionResult<UserStreakDto>> GetUserStreakAsync()
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
            return Unauthorized(new { message = "ID пользователя не найден в токене" });

        var result = await _calendarService.GetUserStreakAsync(userId.Value);
        return Ok(result);
    }

    private int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return null;
        return userId;
    }
}