using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thot_projet.Data;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // HttpClient vers le microservice Auth (FastAPI)
        private static readonly HttpClient httpAuth = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8005/") // même port que uvicorn auth_service
        };

        // DTO pour désérialiser la réponse du microservice (UserOut)
        private class AuthUserDto
        {
            public int id { get; set; }
            public string Email { get; set; }
            public string Nomcomplet { get; set; }
            public string Role { get; set; }
        }

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
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? "").Trim().ToLower();
            var pass = (model.Motdepasse ?? "").Trim();
            var role = (model.Role ?? "").Trim();

            // --------- 1) Tentative via microservice Auth (FastAPI) ----------
            try
            {
                var payload = new
                {
                    Email = email,
                    Motdepasse = pass
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await httpAuth.PostAsync("auth/login", content);

                if (resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    var dto = JsonConvert.DeserializeObject<AuthUserDto>(body);

                    if (dto == null)
                    {
                        ModelState.AddModelError("", "Réponse invalide du microservice Auth.");
                        return View(model);
                    }

                    // Vérifier le rôle choisi par l’utilisateur
                    if (!string.Equals(dto.Role ?? "", role, StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("", "Rôle incorrect pour cet utilisateur.");
                        return View(model);
                    }

                    // Session d'appli + cookie forms (comme avant)
                    FormsAuthentication.SetAuthCookie(dto.Email, model.Mesouvenir);
                    Session["UserId"] = dto.id;
                    Session["UserRole"] = dto.Role;
                    Session["UserName"] = string.IsNullOrWhiteSpace(dto.Nomcomplet)
                                            ? dto.Email
                                            : dto.Nomcomplet;

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return string.Equals(dto.Role ?? "", "Tuteur", StringComparison.OrdinalIgnoreCase)
                        ? RedirectToAction("Dashboard", "Tuteur")
                        : RedirectToAction("Dashboard", "Etudiant");
                }
                else if ((int)resp.StatusCode == 401)
                {
                    // Identifiants invalides
                    ModelState.AddModelError("", "Identifiants incorrects.");
                    return View(model);
                }
                else
                {
                    // Autre erreur du microservice
                    ModelState.AddModelError("", "Erreur Auth microservice (" + (int)resp.StatusCode + ").");
                    // On laisse tomber ici, on ne fait pas fallback pour ne pas confondre
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                // Si le microservice est down, tu peux décider de faire fallback EF
                // ou juste afficher un message. Ici on affiche un message simple.
                ModelState.AddModelError("", "Microservice Auth indisponible : " + ex.Message);
                return View(model);
            }
        }

        // ---- Inscription ----------------------------------------------------
        [AllowAnonymous]
        public ActionResult Register() => View(new RegisterViewModel());

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? "").Trim().ToLower();
            var nom = (model.Nomcomplet ?? "").Trim();
            var role = (model.Role ?? "").Trim();
            var pass = (model.Motdepasse ?? "").Trim();

            // --------- 1) Tentative via microservice Auth (FastAPI) ----------
            try
            {
                var payload = new
                {
                    Email = email,
                    Nomcomplet = nom,
                    Role = role,
                    Motdepasse = pass
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await httpAuth.PostAsync("auth/register", content);

                if (resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    var dto = JsonConvert.DeserializeObject<AuthUserDto>(body);

                    if (dto == null)
                    {
                        ModelState.AddModelError("", "Réponse invalide du microservice Auth.");
                        return View(model);
                    }

                    // Connexion directe post-inscription (comme avant)
                    FormsAuthentication.SetAuthCookie(dto.Email, false);
                    Session["UserId"] = dto.id;
                    Session["UserRole"] = dto.Role;
                    Session["UserName"] = string.IsNullOrWhiteSpace(dto.Nomcomplet)
                                            ? dto.Email
                                            : dto.Nomcomplet;

                    return string.Equals(dto.Role ?? "", "Tuteur", StringComparison.OrdinalIgnoreCase)
                        ? RedirectToAction("Dashboard", "Tuteur")
                        : RedirectToAction("Dashboard", "Etudiant");
                }
                else if ((int)resp.StatusCode == 409)
                {
                    // Email déjà utilisé (géré par le microservice)
                    ModelState.AddModelError("Email", "Ce courriel est déjà utilisé (microservice).");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError("", "Erreur Auth microservice (" + (int)resp.StatusCode + ").");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                // Microservice down -> on peut *optionnellement* faire un fallback EF si tu veux
                // Mais pour rester simple, on signale l'erreur.
                ModelState.AddModelError("", "Microservice Auth indisponible : " + ex.Message);
                return View(model);
            }
        }

        // Génération d’un mot de passe (admin / premier usage) – garde EF ici
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
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
