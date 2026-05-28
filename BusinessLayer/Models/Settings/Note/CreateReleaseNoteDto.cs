using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.Settings.Note
{
    public class CreateReleaseNoteDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Version { get; set; } = string.Empty;

        // Изображения с аннотациями
        public List<IFormFile> Images { get; set; } = new();
        public List<string?> Annotations { get; set; } = new();
    }
}
