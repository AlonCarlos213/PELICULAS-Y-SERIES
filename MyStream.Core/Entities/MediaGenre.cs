using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class MediaGenre
{
    public required Guid MediaItemId { get; set; }
    public required int GenreId { get; set; }
    
    [ForeignKey("MediaItemId")]
    public MediaItem MediaItem { get; set; } = null!;
    
    [ForeignKey("GenreId")]
    public Genre Genre { get; set; } = null!;
}
