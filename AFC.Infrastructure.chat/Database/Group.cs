using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFC.Infrastructure.chat.Database
{
    /// <summary>
    /// Соответствует таблице Groups
    /// </summary>
    public class Group
    {
        [Required]
        [Column(TypeName = "uniqueidentifier")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid GroupId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string? GroupName { get; set; }
    }
}