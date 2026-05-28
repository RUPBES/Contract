using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Models.EXTRA
{ //todo: переместить в service 1
    public class UpdateReleaseNote
    {
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        // Новые изображения (добавляются к существующим)
        //public List<IFormFile> NewImages { get; set; } = new();
        public List<string?> NewAnnotations { get; set; } = new();

        // ID изображений для удаления
        public List<int> DeleteImageIds { get; set; } = new();
    }
}
