using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    public class GenresDto
    {
        [Required]
        public string Name { get; set; }
    }
}
