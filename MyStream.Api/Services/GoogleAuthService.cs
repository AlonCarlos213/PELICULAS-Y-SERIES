using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyStream.Infrastructure.Data;
using MyStream.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MyStream.Api.Services;

public interface IGoogleAuthService
{
    Task<string> AuthenticateWithGoogleAsync(string idToken);
}

public class GoogleAuthService : IGoogleAuthService
{
    private readonly MyStreamDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        MyStreamDbContext context,
        IConfiguration configuration,
        ILogger<GoogleAuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> AuthenticateWithGoogleAsync(string idToken)
    {
        try
        {
            // Verify Google token
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            });

            if (payload == null)
            {
                throw new InvalidOperationException("Invalid Google token");
            }

            // Find or create user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = payload.Email,
                    Username = payload.Email.Split('@')[0],
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    GoogleId = payload.Subject,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Created new user: {user.Email}");
            }
            else if (user.GoogleId == null)
            {
                // Link Google account to existing user
                user.GoogleId = payload.Subject;
                user.FirstName = user.FirstName ?? payload.GivenName;
                user.LastName = user.LastName ?? payload.FamilyName;
                user.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Linked Google account to existing user: {user.Email}");
            }

            // Generate JWT token
            return GenerateJwtToken(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            throw;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? ""),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("google_id", user.GoogleId ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
