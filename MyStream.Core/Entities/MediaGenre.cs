using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStream.Core.Entities;

public class MediaGenre
{
    public Guid MediaItemId { get; set; }
    public int GenreId { get; set; }
    public virtual MediaItem MediaItem { get; set; } = null!;
    public virtual Genre Genre { get; set; } = null!;
}
