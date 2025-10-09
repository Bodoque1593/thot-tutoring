using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class SessionClavardage
    {

        [Key] public int id { get; set; }
        public int EtudiantId { get; set; }
        public int TuteurId { get; set; }
        public DateTime DemarreLe { get; set; }
        public DateTime? TermineLe { get; set; }
        public int DureeMinutes { get; set; }

        public Utilisateur Etudiant { get; set; }
        public Utilisateur Tuteur { get; set; }

    }
}