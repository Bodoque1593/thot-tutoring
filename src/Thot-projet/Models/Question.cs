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
        [Required(ErrorMessage = "L'etudiant est requis")]
        public int EtudiantId { get; set; }
        [Required(ErrorMessage = "Le cours est requis")]
        public int CoursId { get; set; }
        [Required(ErrorMessage = "La ressource est requise")]
        public int RessourceId { get; set; }
        [Required(ErrorMessage = "Le contenu est requis")]
        [StringLength(2000,ErrorMessage ="2000 caracteres max")]
        public string Contenu { get; set; }
        public bool EstResolvee { get; set; }
        public DateTime Creele { get; set; }

        public Utilisateur Etudiant { get; set; } // padre 1
        public Cours Cours { get; set; } // padre 1
        public Ressource Ressource { get; set; } // padre 1


        public ICollection<Reponse> Reponses { get; set; } = new List<Reponse>(); // hijo muchos



    }
}