using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AFC.Infrastructure.chat.Database
{
    /// <summary>
    /// Соответствует таблице Users
    /// </summary>
    public class User
    {
        [Required]
        [Column(TypeName = "uniqueidentifier")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public required string UserName { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(50)")]
        public required string Password { get; set; }
    }
}