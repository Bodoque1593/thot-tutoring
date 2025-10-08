using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class ProfilTuteur
    {

        [Key] public int id { get; set; }
        public int UtilisateurId { get; set; }
        public string Sujets { get; set; }
        public string Niveaux { get; set; }
    }
}