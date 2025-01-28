﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFC.Infrastructure.chat.Database
{
    /// <summary>
    /// Соответствует таблице Files
    /// </summary>
    public class File
    {
        [Required]
        [Column(TypeName = "UUID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        /// <summary>
        /// Хранит файл в двоичной кодировке
        /// </summary>
        [Required]
        //[Column(TypeName = "varbinary")]
        public required byte[] FileData { get; set; }
        /// <summary>
        /// Дата добавления файла. 
        /// Необходима для автоочистки таблицы по сроку давности файлов.
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateOnly Date { get; set; }
    }
}