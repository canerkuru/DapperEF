using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class ArtistDto
    {
        [Required]
        public string Name { get; set; }
    }
}
