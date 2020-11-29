
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Genres
    {

        [Key]
        public int GenreId { get; set; }

        public string Name { get; set; }
    }
}
