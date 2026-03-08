using Microsoft.EntityFrameworkCore;
using MyStream.Core.Entities;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace MyStream.Infrastructure.Data;

public class MyStreamDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public MyStreamDbContext(DbContextOptions<MyStreamDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    // Módulo A - Seguridad
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSession> UserSessions { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    // Módulo B - Catálogo
    public DbSet<Library> Libraries { get; set; } = null!;
    public DbSet<MediaItem> MediaItems { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<MediaGenre> MediaGenres { get; set; } = null!;

    // Módulo C - Series
    public DbSet<Season> Seasons { get; set; } = null!;
    public DbSet<Episode> Episodes { get; set; } = null!;

    // Módulo D - IPTV
    public DbSet<LiveChannel> LiveChannels { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<ChannelCategory> ChannelCategories { get; set; } = null!;

    // Módulo E - Sync
    public DbSet<WatchHistory> WatchHistories { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Default to PostgreSQL for all environments
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection") ?? 
                "Host=localhost;Database=mystream;Username=postgres;Password=password");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar timestamps para PostgreSQL
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.GoogleId).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.GoogleId).IsUnique();
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ExpirationDate).HasColumnType("timestamp with time zone");
            entity.Property(e => e.TokenJWT).HasMaxLength(1000).IsRequired();
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<Library>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<MediaItem>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.PosterUrl).HasMaxLength(1000);
            entity.Property(e => e.BackdropUrl).HasMaxLength(1000);
            entity.Property(e => e.Platform).HasMaxLength(100);
            entity.HasOne(e => e.Library).WithMany().HasForeignKey(e => e.LibraryId);
            entity.HasMany(e => e.Genres).WithMany();
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.HasOne(e => e.MediaItem).WithMany().HasForeignKey(e => e.MediaItemId);
        });

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.LocalPath).HasMaxLength(1000);
            entity.HasOne(e => e.Season).WithMany().HasForeignKey(e => e.SeasonId);
        });

        modelBuilder.Entity<LiveChannel>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.StreamUrl).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.LogoUrl).HasMaxLength(1000);
            entity.HasMany(e => e.Categories).WithMany();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ChannelCategory>(entity =>
        {
            entity.HasOne(e => e.LiveChannel).WithMany();
            entity.HasOne(e => e.Category).WithMany();
        });

        modelBuilder.Entity<WatchHistory>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.MediaItem).WithMany().HasForeignKey(e => e.MediaItemId);
        });

        modelBuilder.Entity<MediaGenre>()
            .HasOne(mg => mg.MediaItem)
            .WithMany()
            .HasForeignKey(mg => mg.MediaItemId);
    }
}
