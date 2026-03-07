using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyStream.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

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
    
    // Auto-create database and apply migrations in development
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyStreamDbContext>();
    context.Database.Migrate();
}
else
{
    // Apply migrations in production as well
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MyStreamDbContext>();
        
        // Configure warnings to suppress pending model changes warning
        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
        
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
        throw;
    }
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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