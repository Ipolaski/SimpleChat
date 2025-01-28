using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFC.Infrastructure.chat.Database
{
    public class GroupConnection
    {       
        [Required]
        [Column(TypeName = "UUID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }       
        public required Group Group { get; set; }
        public required User User { get; set; }
    }
}