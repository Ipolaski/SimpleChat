using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.AFC.Infrastructure.Database.Entities
{
    /// <summary>
    /// Соответствует таблице Groups
    /// </summary>
    public class Group
    {
        [Required]
        [Column(TypeName = "UUID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string? Name { get; set; }
		public User Owner { get; set; }
		public Guid OwnerId { get; set; }
		public ICollection<User> Members { get; set; } = [];
	}
}