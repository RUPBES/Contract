using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.Settings.Note
{
    public class UpdateReleaseNoteDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Version { get; set; } = string.Empty;

        // Новые изображения (добавляются к существующим)
        public List<IFormFile> NewImages { get; set; } = new();
        public List<string?> NewAnnotations { get; set; } = new();

        // ID изображений для удаления
        public List<int> DeleteImageIds { get; set; } = new();
    }
}
