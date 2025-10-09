using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class ModuleCours
    {

        [Key] public int id { get; set; }

        public int CoursId { get; set; }

  
        public Cours Cours { get; set; } 

        public int Numero { get; set; }
        public string Titre { get; set; }

        public ICollection<Ressource> Ressources { get; set; } = new List<Ressource>(); // padre 1

    }
}