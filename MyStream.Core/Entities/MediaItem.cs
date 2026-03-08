using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class MediaItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public MediaType MediaType { get; set; }
    public string? Platform { get; set; }
    public required Guid LibraryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    [ForeignKey("LibraryId")]
    public Library Library { get; set; } = null!;
    
    public ICollection<MediaGenre> MediaGenres { get; set; } = new List<MediaGenre>();
    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    public ICollection<WatchHistory> WatchHistories { get; set; } = new List<WatchHistory>();
}

public enum MediaType
{
    Movie = 0,
    Series = 1,
    Documentary = 2,
    Anime = 3
}
