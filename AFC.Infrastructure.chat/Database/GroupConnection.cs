using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFC.Infrastructure.chat.Database
{
    public class GroupConnection
    {
        [Key]
        [Required]
        [Column(TypeName = "uniqueidentifier")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ConnectionId { get; set; }

        [Required]
        public required Guid GroupId { get; set; }
        public required Group Group { get; set; }

        [Required]
        public required Guid UserId { get; set; }
        public required User User { get; set; }

        [Required]
        public required bool IsAdmin { get; set; }
    }
}