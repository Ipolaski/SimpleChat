﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFC.Infrastructure.chat.Database
{
    public class Messages
    {
        [Required]
        [Column(TypeName = "UUID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }       
        public required Group Group { get; set; }
        public required User User { get; set; }
        public string? Text { get; set; }
        public File? File { get; set; }
        public required DateTime DateTime { get; set; }
    }
}