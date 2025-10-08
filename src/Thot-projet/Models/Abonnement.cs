using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Abonnement
    {
        [Key] public int id { get; set; }
        public int UtilisateurId { get; set; }
        public string Type { get; set; }
        public DateTime? DebutLe { get; set; }
        public DateTime? ExpireLe { get; set; }
        public bool Actif { get; set; }
    }
}