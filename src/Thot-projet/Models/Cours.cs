using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Thot_projet.Models
{
    public class Cours
    {
        [Key] public int id { get; set; }
        public string Nom { get; set; }
        public string Niveau { get; set; }



    }
}