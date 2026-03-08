using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyStream.Infrastructure.Data;
using MyStream.Api.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MyStream API", 
        Version = "v1",
        Description = "API para streaming de películas y series con autenticación Google OAuth"
    });
    
    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese su token JWT en el formato: Bearer {token}"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Register custom services
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// Configure DbContext based on environment
if (builder.Environment.IsProduction())
{
    // Use PostgreSQL for production (Railway)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found in production environment");
    }
    
    Console.WriteLine($"Using PostgreSQL connection: {connectionString}");
    
    // Fix Railway connection string format
    var fixedConnectionString = connectionString.Replace("postgresql://", "Host=");
    var uri = new Uri(connectionString);
    var dbConnectionString = $"Host={uri.Host}:{uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]}";
    
    Console.WriteLine($"Fixed connection string: {dbConnectionString}");
    
    builder.Services.AddDbContext<MyStreamDbContext>(options =>
{
    options.UseNpgsql(dbConnectionString);
    options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
});
}
else
{
    // Use SQLite for development
    builder.Services.AddDbContext<MyStreamDbContext>(options =>
        options.UseSqlite("Data Source=mystream.db"));
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // Auto-create database and apply migrations// Apply any pending migrations
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MyStreamDbContext>();
        await context.Database.MigrateAsync();
        
        // Clean table for testing
        try
        {
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" RESTART IDENTITY CASCADE;");
            Console.WriteLine("Users table cleaned for testing");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning Users table: {ex.Message}");
        }
    }
}
else
{
    // Apply migrations in production as well
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MyStreamDbContext>();
        
        // Force reset: Drop migration history and Users table
        try
        {
            await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"__EFMigrationsHistory\" CASCADE;");
            Console.WriteLine("Migration history table dropped");
            
            await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"Users\" CASCADE;");
            Console.WriteLine("Users table dropped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error dropping tables: {ex.Message}");
        }
        
        // Apply migrations - this will recreate everything with correct schema
        await context.Database.MigrateAsync();
        Console.WriteLine("Migrations applied - Users table created with correct schema");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
        throw;
    }
}

app.UseHttpsRedirection();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyStream.Api v1");
    options.RoutePrefix = string.Empty;
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapControllers();

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}