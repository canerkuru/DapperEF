using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class AlbumDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int ArtistId { get; set; }
    }

    public class AlbumArtistDto
    {
        [Required]
        public string TitleAlbum { get; set; }

        [Required]
        public string ArtistName { get; set; }
    }

}
