using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Thot_projet.Models
{
    public class Ressource
    {
        [Key] public int id { get; set; }

        [Required(ErrorMessage = "Le module est requis")]
        public int ModuleCoursId { get; set; }

        [Required(ErrorMessage = "Le type est requis")]
        [StringLength(50, ErrorMessage = "50 caracteres max")]
        public string Type { get; set; }

        [Required(ErrorMessage = " Le titre est requis")]
        [StringLength(100, ErrorMessage = "100 caracteres max")]
        public string Titre { get; set; }

        // ⬇️ IMPORTANTE:
        // - Quitamos [Required] y [Url] para permitir:
        //   a) URL absoluta (YouTube, etc.)   b) Ruta relativa de un PDF subido (/Content/...)
        [StringLength(255, ErrorMessage = "255 caracteres max")]
        public string url { get; set; }

        public ModuleCours ModuleCours { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
