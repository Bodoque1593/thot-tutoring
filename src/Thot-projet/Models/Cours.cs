using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thot_projet.Models
{
    public class Cours
    {
        [Key] public int id { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(150, ErrorMessage = "150 caractères maximum")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le niveau est requis")]
        [StringLength(50, ErrorMessage = "50 caractères maximum")]
        public string Niveau { get; set; }

        // NUEVO: columnas que ya existen en tu BD (Prix NOT NULL) 
        [Required]
        [Range(0, 999999)]
        [Display(Name = "Prix")]
        [Column("Prix")]
        public decimal Prix { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "URL invalide")]
        [Display(Name = "Image (URL)")]
        public string ImageUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        // Relaciones
        public virtual ICollection<ModuleCours> Modules { get; set; } = new List<ModuleCours>();
        public virtual ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
