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
        [Column(TypeName = "UUID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Password { get; set; }
		[Required]
		[Column()]
		public required bool IsAdmin { get; set; }

		public ICollection<Group> OwnedGroups{ get; set; }
		public ICollection<Group> BeInGroups { get; set; }
	}
}