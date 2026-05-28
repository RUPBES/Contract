using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Models.Settings.Note
{
    public class ReleaseNoteListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public bool IsRead { get; set; }
        public int ImagesCount { get; set; }
    }
}
