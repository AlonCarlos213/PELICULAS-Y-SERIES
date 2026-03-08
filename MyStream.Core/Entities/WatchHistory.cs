using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class WatchHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid MediaItemId { get; set; }
    public int LastSeconds { get; set; }
    public DateTime LastWatched { get; set; }
    public bool IsFinished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    [ForeignKey("MediaItemId")]
    public MediaItem MediaItem { get; set; } = null!;
}
