using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFC.Infrastructure.chat.Database
{
    public class Messages
    {
        [Required]
        [Column(TypeName = "uniqueidentifier")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MessageId { get; set; }

        [Required]
        public required Guid GroupId { get; set; }
        public required Group Group { get; set; }

        [Required]
        public required Guid UserId { get; set; }
        public required User User { get; set; }

        [Column(TypeName = "text")]
        public string? Text { get; set; }

        public Guid? FileId { get; set; }
        public File? File { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public required DateTime DateTime { get; set; }
    }
}