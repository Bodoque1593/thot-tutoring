using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Utilisateur
    {
        [Key] public int id { get; set; }
        public string Nomcomplet { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime Creele { get; set; }

        [Column("Motdepasse")]               // <- nombre real en la BD
        [Display(Name = "Mot de passe")]
        public string  Motdepasse { get; set; }


        public virtual ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<Reponse> Reponses { get; set; } = new List<Reponse>();
        public virtual ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();

        public virtual Abonnement Abonnement { get; set; }
        public virtual ProfilTuteur ProfilTuteur { get; set; }





    }
}