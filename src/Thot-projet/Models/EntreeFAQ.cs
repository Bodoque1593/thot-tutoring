using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class EntreeFAQ
    {

        [Key] public int id { get; set; }
        public string QuestionTexte { get; set; }
        public string ReponseTexte { get; set; }
        public DateTime PublieLe { get; set; }
    }
}