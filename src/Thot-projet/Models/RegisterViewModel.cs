using System.ComponentModel.DataAnnotations;

namespace Thot_projet.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Courriel")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Nom complet")]
        public string Nomcomplet { get; set; }

        [Required]
        [Display(Name = "Rôle")]
        public string Role { get; set; } // "Etudiant" | "Tuteur"

        [Required, DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Motdepasse { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Motdepasse", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmMotdepasse { get; set; }
    }
}
