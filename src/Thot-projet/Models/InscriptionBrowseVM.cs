using System;

namespace Thot_projet.Models
{
    public class InscriptionBrowseVM
    {
        public int CoursId { get; set; }
        public string Nom { get; set; }
        public string Niveau { get; set; }
        public bool DejaInscrit { get; set; }
    }
}
