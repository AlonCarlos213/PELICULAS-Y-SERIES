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
            Console.WriteLine($"Attempting to validate Google token...");
            
            // Verify Google token with basic validation
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Google:ClientId"] }
            };

            Console.WriteLine($"Validating with ClientId: {_configuration["Google:ClientId"]}");
            
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload == null)
            {
                Console.WriteLine("Google token validation returned null payload");
                throw new InvalidOperationException("Invalid Google token - null payload");
            }

            Console.WriteLine($"Token validated successfully for user: {payload.Email}");
            Console.WriteLine($"Token subject: {payload.Subject}");
            Console.WriteLine($"Extracted email from Google token: {payload.Email}");

            // Ensure database context is ready
            if (_context.Database.IsRelational())
            {
                Console.WriteLine("Database context is ready and connected");
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
                    GoogleId = payload.Subject,
                    DisplayName = $"{payload.GivenName} {payload.FamilyName}".Trim(),
                    ProfilePicture = payload.Picture,
                    Role = UserRole.User
                };

                _context.Users.Add(user);
                
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Created new user: {user.Email}");
                    Console.WriteLine($"Created new user in database: {user.Email}");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"Database error creating user: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner database exception: {dbEx.InnerException.Message}");
                        Console.WriteLine($"Inner exception type: {dbEx.InnerException.GetType().Name}");
                    }
                    throw new InvalidOperationException($"Failed to create user: {dbEx.InnerException?.Message ?? dbEx.Message}", dbEx);
                }
            }
            else
            {
                Console.WriteLine($"Existing user found: {user.Email}");
            }

            // Generate JWT token
            var jwtToken = GenerateJwtToken(user);
            Console.WriteLine($"Generated JWT token for user: {user.Email}");
            
            return jwtToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error during Google authentication: {ex.Message}");
            Console.WriteLine($"Error type: {ex.GetType().Name}");
            Console.WriteLine($"Error stack trace: {ex.StackTrace}");
            
            // Check for DbUpdateException specifically
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Console.WriteLine($"Database update exception: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner database exception: {dbEx.InnerException.Message}");
                    Console.WriteLine($"Inner exception type: {dbEx.InnerException.GetType().Name}");
                }
            }
            
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
            new Claim("display_name", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("google_id", user.GoogleId)
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
