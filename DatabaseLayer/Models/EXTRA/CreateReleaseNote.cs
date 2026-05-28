using DatabaseLayer.Models.KDO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Models.EXTRA
{
    public class CreateReleaseNote
    {//todo: переместить в service 1
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        // Изображения с аннотациями
        public List<string?> Annotations { get; set; } = new();
    }
}
