using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace Thot_projet.Models
{
    public class Question
    {

        [Key] public int id { get; set; }
        public int EtudiantId { get; set; }
        public int CoursId { get; set; }
        public int RessourceId { get; set; }
        public string Contenu { get; set; }
        public bool EstResolvee { get; set; }
        public DateTime Creele { get; set; }

        public Utilisateur Etudiant { get; set; } // padre 1
        public Cours Cours { get; set; } // padre 1
        public Ressource Ressource { get; set; } // padre 1


        public ICollection<Reponse> Reponses { get; set; } = new List<Reponse>(); // hijo muchos



    }
}