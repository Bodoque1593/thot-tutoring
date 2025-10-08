using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Inscription
    {
        [Key] public int id { get; set; }
        public int UtilisateurId { get; set; }
        public int CoursId { get; set; }
        public DateTime InscritLe { get; set; }

    }
}