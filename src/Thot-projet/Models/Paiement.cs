using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Paiement
    {

        [Key] public int id { get; set; }
        public int UtilisateurId { get; set; }
        public decimal Montant { get; set; }
        public string Monnaie { get; set; }
        public string Statut { get; set; }
        public DateTime PayeLe { get; set; }
    }
}