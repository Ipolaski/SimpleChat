using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.AFC.Infrastructure.Database.Entities
{
    public class Message
    {
        [Required]
        [Column(TypeName = "UUID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }
        public required Group Group { get; set; }
        public string Username { get; set; }
        public string? Text { get; set; }
        public File? File { get; set; }
        public required DateTime DateTime { get; set; }
    }
}