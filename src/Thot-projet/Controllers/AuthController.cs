using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Thot_projet.Data;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Générateur crypto-sûr côté serveur
        private static string GenererMotDePasse(int longueur = 12)
        {
            const string alpha = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@#$%*-_";

            string alphabet = alpha + digits + symbols;

            var bytes = new byte[longueur];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);

            var sb = new StringBuilder(longueur);
            for (int i = 0; i < longueur; i++)
            {
                int index = bytes[i] % alphabet.Length;
                sb.Append(alphabet[index]);
            }
            return sb.ToString();
        }

        // GET: /Auth/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? "").Trim().ToLower();
            var pass = (model.Motdepasse ?? "").Trim();
            var role = (model.Role ?? "").Trim();

            var user = db.Utilisateurs.FirstOrDefault(u => u.Email.ToLower() == email);

            // Vérifie utilisateur / mot de passe / rôle
            if (user == null)
            {
                ModelState.AddModelError("", "Utilisateur inexistant.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(user.Motdepasse) ||
                !string.Equals(user.Motdepasse.Trim(), pass, StringComparison.Ordinal))
            {
                ModelState.AddModelError("", "Mot de passe invalide.");
                return View(model);
            }

            if (!string.Equals(user.Role ?? "", role, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Rôle incorrect pour cet utilisateur.");
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(user.Email, model.Mesouvenir);

            Session["UserId"] = user.id;
            Session["UserRole"] = user.Role; // "Tuteur" | "Etudiant"
            Session["UserName"] = string.IsNullOrWhiteSpace(user.Nomcomplet) ? user.Email : user.Nomcomplet;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Cours");
        }

        // POST: /Auth/GenererMdp
        // Génère et enregistre un mot de passe pour l'utilisateur (ex: 1ère connexion)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult GenererMdp(string email, string role)
        {
            var mail = (email ?? "").Trim().ToLower();
            var user = db.Utilisateurs.FirstOrDefault(u => u.Email.ToLower() == mail);

            if (user == null)
            {
                TempData["err"] = "Courriel inexistant.";
                return RedirectToAction("Login");
            }

            if (!string.IsNullOrWhiteSpace(role) &&
                !string.Equals(user.Role ?? "", role.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                TempData["err"] = "Le rôle ne correspond pas à l'utilisateur.";
                return RedirectToAction("Login");
            }

            var nouveau = GenererMotDePasse(12);
            user.Motdepasse = nouveau;
            db.SaveChanges();

            TempData["ok"] = $"Mot de passe généré pour {user.Email}.";
            TempData["mdp"] = nouveau; // On l’affiche une seule fois

            // Pré-remplit l’email/role à l’écran de login
            return RedirectToAction("Login", new { returnUrl = "" });
        }

        // GET: /Auth/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
