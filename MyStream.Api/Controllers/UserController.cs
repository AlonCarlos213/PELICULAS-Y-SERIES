using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStream.Api.DTOs;
using MyStream.Infrastructure.Data;
using System.Security.Claims;

namespace MyStream.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly MyStreamDbContext _context;

    public UserController(MyStreamDbContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
        {
            return Unauthorized("Invalid user ID in token");
        }

        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new UserProfileDto
            {
                DisplayName = u.DisplayName,
                Email = u.Email,
                ProfilePicture = u.ProfilePicture
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
    }

    [HttpPatch("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto updateDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
        {
            return Unauthorized("Invalid user ID in token");
        }

        if (string.IsNullOrEmpty(updateDto.DisplayName))
        {
            return BadRequest("DisplayName is required");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound("User not found");
        }

        // Validar que solo se pueda editar el DisplayName
        // Email y ProfilePicture son campos de solo lectura (de Google)
        if (!string.IsNullOrEmpty(updateDto.DisplayName))
        {
            // Solo se permite modificar el DisplayName
            user.DisplayName = updateDto.DisplayName;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Return updated profile
        var updatedProfile = new UserProfileDto
        {
            DisplayName = user.DisplayName,
            Email = user.Email,
            ProfilePicture = user.ProfilePicture
        };

        return Ok(updatedProfile);
    }
}
