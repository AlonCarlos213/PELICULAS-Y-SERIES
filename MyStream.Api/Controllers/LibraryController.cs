using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyStream.Api.DTOs;
using MyStream.Core.Entities;
using MyStream.Infrastructure.Data;
using System.Security.Claims;

namespace MyStream.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LibraryController : ControllerBase
{
    private readonly MyStreamDbContext _context;

    public LibraryController(MyStreamDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<LibraryDto>> CreateLibrary([FromBody] CreateLibraryDto request)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            return BadRequest("Library name is required");
        }

        if (string.IsNullOrEmpty(request.LocalPath))
        {
            return BadRequest("Local path is required");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var adminId))
        {
            return Unauthorized("Invalid admin ID in token");
        }

        // Verificar si ya existe una librería con el mismo nombre para este admin
        var existingLibrary = await _context.Libraries
            .FirstOrDefaultAsync(l => l.Name == request.Name && l.CreatedBy == adminId);

        if (existingLibrary != null)
        {
            return Conflict("A library with this name already exists");
        }

        var library = new Library
        {
            Name = request.Name,
            LocalPath = request.LocalPath,
            IsActive = true,
            CreatedBy = adminId
        };

        _context.Libraries.Add(library);
        await _context.SaveChangesAsync();

        var libraryDto = new LibraryDto
        {
            Id = library.Id,
            Name = library.Name,
            LocalPath = library.LocalPath,
            IsActive = library.IsActive,
            CreatedAt = library.CreatedAt
        };

        return CreatedAtAction(nameof(GetLibrary), new { id = library.Id }, libraryDto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibraryDto>>> GetLibraries()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var adminId))
        {
            return Unauthorized("Invalid admin ID in token");
        }

        var libraries = await _context.Libraries
            .Where(l => l.CreatedBy == adminId)
            .Select(l => new LibraryDto
            {
                Id = l.Id,
                Name = l.Name,
                LocalPath = l.LocalPath,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(libraries);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LibraryDto>> GetLibrary(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var adminId))
        {
            return Unauthorized("Invalid admin ID in token");
        }

        var library = await _context.Libraries
            .FirstOrDefaultAsync(l => l.Id == id && l.CreatedBy == adminId);

        if (library == null)
        {
            return NotFound();
        }

        var libraryDto = new LibraryDto
        {
            Id = library.Id,
            Name = library.Name,
            LocalPath = library.LocalPath,
            IsActive = library.IsActive,
            CreatedAt = library.CreatedAt
        };

        return Ok(libraryDto);
    }
}
