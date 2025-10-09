using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Reponse
    {
        [Key] public int id { get; set; }
        public int QuestionId { get; set; }
        public int TuteurId { get; set; }
        public string Contenu { get; set; }
        public DateTime Creele { get; set; }

        public Question Question { get; set; } // padre 1
        public Utilisateur Tuteur { get; set; } // padre 1


    }
}