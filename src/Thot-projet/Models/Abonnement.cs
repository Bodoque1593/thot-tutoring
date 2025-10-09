using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Abonnement
    {
       
        [Key, ForeignKey("Utilisateur")]
        public int UtilisateurId { get; set; }
        public virtual Utilisateur Utilisateur { get; set; }

        [Required]
        public string Type { get; set; }  

        public DateTime? DebutLe { get; set; }
        public DateTime? ExpireLe { get; set; }
        public bool Actif { get; set; }
    }
}