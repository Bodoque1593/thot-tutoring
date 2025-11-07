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

        // Génère un mot de passe lisible (12 chars) – usage ponctuel (admin / premier accès)
        private static string GenererMotDePasse(int longueur = 12)
        {
            const string alpha = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@#$%*-_";
            string alphabet = alpha + digits + symbols;

            var bytes = new byte[longueur];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);

            var sb = new StringBuilder(longueur);
            for (int i = 0; i < longueur; i++) sb.Append(alphabet[bytes[i] % alphabet.Length]);
            return sb.ToString();
        }

        // ---- Connexion ------------------------------------------------------
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? "").Trim().ToLower();
            var pass = (model.Motdepasse ?? "").Trim();
            var role = (model.Role ?? "").Trim();

            var user = db.Utilisateurs.FirstOrDefault(u => u.Email.ToLower() == email);
            if (user == null) { ModelState.AddModelError("", "Utilisateur inexistant."); return View(model); }
            if (string.IsNullOrWhiteSpace(user.Motdepasse) || !string.Equals(user.Motdepasse.Trim(), pass, StringComparison.Ordinal))
            { ModelState.AddModelError("", "Mot de passe invalide."); return View(model); }
            if (!string.Equals(user.Role ?? "", role, StringComparison.OrdinalIgnoreCase))
            { ModelState.AddModelError("", "Rôle incorrect pour cet utilisateur."); return View(model); }

            // Session d'appli + cookie forms
            FormsAuthentication.SetAuthCookie(user.Email, model.Mesouvenir);
            Session["UserId"] = user.id;
            Session["UserRole"] = user.Role;
            Session["UserName"] = string.IsNullOrWhiteSpace(user.Nomcomplet) ? user.Email : user.Nomcomplet;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return string.Equals(user.Role ?? "", "Tuteur", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction("Dashboard", "Tuteur")
                : RedirectToAction("Dashboard", "Etudiant");
        }

        // ---- Inscription ----------------------------------------------------
        [AllowAnonymous]
        public ActionResult Register() => View(new RegisterViewModel());

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? "").Trim().ToLower();
            if (db.Utilisateurs.Any(u => u.Email.ToLower() == email))
            { ModelState.AddModelError("Email", "Ce courriel est déjà utilisé."); return View(model); }

            var user = new Utilisateur
            {
                Email = email,
                Nomcomplet = (model.Nomcomplet ?? "").Trim(),
                Role = (model.Role ?? "").Trim(),
                Motdepasse = (model.Motdepasse ?? "").Trim(),  // texte brut (hypothèse du cours)
                Creele = DateTime.UtcNow
            };
            db.Utilisateurs.Add(user);
            db.SaveChanges();

            // Connexion directe post-inscription
            FormsAuthentication.SetAuthCookie(user.Email, false);
            Session["UserId"] = user.id; Session["UserRole"] = user.Role;
            Session["UserName"] = string.IsNullOrWhiteSpace(user.Nomcomplet) ? user.Email : user.Nomcomplet;

            return string.Equals(user.Role ?? "", "Tuteur", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction("Dashboard", "Tuteur")
                : RedirectToAction("Dashboard", "Etudiant");
        }

        // Génération d’un mot de passe (admin / premier usage)
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult GenererMdp(string email, string role)
        {
            var mail = (email ?? "").Trim().ToLower();
            var user = db.Utilisateurs.FirstOrDefault(u => u.Email.ToLower() == mail);
            if (user == null) { TempData["err"] = "Courriel inexistant."; return RedirectToAction("Login"); }

            if (!string.IsNullOrWhiteSpace(role) &&
                !string.Equals(user.Role ?? "", role.Trim(), StringComparison.OrdinalIgnoreCase))
            { TempData["err"] = "Le rôle ne correspond pas à l'utilisateur."; return RedirectToAction("Login"); }

            var nouveau = GenererMotDePasse(12);
            user.Motdepasse = nouveau;
            db.SaveChanges();

            TempData["ok"] = $"Mot de passe généré pour {user.Email}.";
            TempData["mdp"] = nouveau; // affiché une seule fois
            return RedirectToAction("Login");
        }

        // ---- Déconnexion ----------------------------------------------------
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
