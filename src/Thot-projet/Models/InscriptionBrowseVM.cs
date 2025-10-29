namespace Thot_projet.Models
{
    public class InscriptionBrowseVM
    {
        public int CoursId { get; set; }
        public string Nom { get; set; }
        public string Niveau { get; set; }          // en tu modelo es string
        public decimal Prix { get; set; }           // Cours.Prix es decimal (NOT NULL)
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public bool DejaInscrit { get; set; }
    }
}
