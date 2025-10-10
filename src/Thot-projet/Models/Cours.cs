using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Cours
    {
        [Key] public int id { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(150, ErrorMessage = "150 caractères maximum")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le niveau est requis")]
        [StringLength(50, ErrorMessage = "50 caractères maximum")]
        public string Niveau { get; set; }


        public ICollection<ModuleCours> Modules { get; set; } = new List<ModuleCours>(); // la clase de aca es el padre y estos son los hijos
        public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();

        public ICollection<Question> Questions { get; set; } = new List<Question>(); 

      


    }
}