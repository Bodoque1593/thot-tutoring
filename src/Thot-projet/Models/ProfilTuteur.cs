using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class ProfilTuteur
    {
  
        [Key, ForeignKey("Utilisateur")]
        public int UtilisateurId { get; set; }
        public virtual Utilisateur Utilisateur { get; set; }

        public string Sujets { get; set; }  
        public string Niveaux { get; set; } 
    }
}