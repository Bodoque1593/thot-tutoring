using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Ressource
    {
        [Key] public int id { get; set; }
        public int ModuleCoursId { get; set; }
        public string Type { get; set; }
        public string Titre { get; set; }
        public string url { get; set; }

        public ModuleCours ModuleCours { get; set; } //hija N

        public ICollection<Question> Questions { get; set; } = new List<Question>(); // hija N



    }
}