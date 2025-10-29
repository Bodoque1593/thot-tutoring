using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Thot_projet.Models
{
    public class Question
    {
        [Key] public int id { get; set; }

        [Required(ErrorMessage = "L'etudiant est requis")]
        public int EtudiantId { get; set; }

        [Required(ErrorMessage = "Le cours est requis")]
        public int CoursId { get; set; }

        // AHORA OPCIONAL (nullable)
        public int? RessourceId { get; set; }

        [Required(ErrorMessage = "Le contenu est requis")]
        [StringLength(2000, ErrorMessage = "2000 caracteres max")]
        public string Contenu { get; set; }

        public bool EstResolvee { get; set; }
        public DateTime Creele { get; set; }

        public Utilisateur Etudiant { get; set; }
        public Cours Cours { get; set; }
        public Ressource Ressource { get; set; }

        public ICollection<Reponse> Reponses { get; set; } = new List<Reponse>();
    }
}
