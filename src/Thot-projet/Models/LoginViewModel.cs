using System.ComponentModel.DataAnnotations;

namespace Thot_projet.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Courriel")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Motdepasse { get; set; }

        [Display(Name = "Se souvenir de moi")]
        public bool Mesouvenir { get; set; }

        [Display(Name = "Rôle")]
        [Required]
        public string Role { get; set; } 
    }
}
