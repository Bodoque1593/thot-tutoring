using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Ressource
    {
        [Key] public int id { get; set; }


        [Required(ErrorMessage ="Le module est requis")]
        public int ModuleCoursId { get; set; }

        [Required(ErrorMessage ="Le type est requis")]
        [StringLength(50, ErrorMessage ="50 caracteres max")]
        public string Type { get; set; }

        [Required(ErrorMessage =" Le titre est requis")]
        [StringLength(100, ErrorMessage ="100 caracteres max")]
        public string Titre { get; set; }

        [Required(ErrorMessage ="Le lien est requis")]
        [StringLength(255, ErrorMessage ="255 caracteres max")]
        [Url(ErrorMessage = "Le lien doit etre une URL valide")]
        public string url { get; set; }

        public ModuleCours ModuleCours { get; set; } //hija N

        public ICollection<Question> Questions { get; set; } = new List<Question>(); // hija N



    }
}