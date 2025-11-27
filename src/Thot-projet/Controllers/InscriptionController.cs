using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Etudiant")]
    public class InscriptionController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // HttpClient pour parler au microservice d’inscriptions (FastAPI)
        private static readonly HttpClient httpInscription = new HttpClient
        {
            // ⚠️ MISMO puerto que usas en uvicorn inscription_service:app --port 8007
            BaseAddress = new Uri("http://127.0.0.1:8007/")
        };

        // Helpers de session
        private int? CurrentUserId() => Session["UserId"] as int?;
        private bool IsEtudiant() =>
            string.Equals(Convert.ToString(Session["UserRole"]), "Etudiant",
                          StringComparison.OrdinalIgnoreCase);

        // ---------------------- MES COURS ----------------------
        public ActionResult Index()
        {
            var uid = CurrentUserId();
            if (uid == null)
                return RedirectToAction("Login", "Auth");

            var list = db.Inscriptions
                         .Include(i => i.Cours)
                         .Where(i => i.UtilisateurId == uid.Value)
                         .OrderByDescending(i => i.InscritLe)
                         .ToList();

            return View(list);
        }

        // ---------------------- CATALOGUE ----------------------
        [HttpGet]
        public ActionResult Browse()
        {
            var uid = CurrentUserId();
            if (uid == null || !IsEtudiant())
                return RedirectToAction("Login", "Auth");

            var misIds = db.Inscriptions
                           .Where(i => i.UtilisateurId == uid.Value)
                           .Select(i => i.CoursId)
                           .ToList();

            var vms = db.Cours
                        .OrderBy(c => c.Nom)
                        .Select(c => new InscriptionBrowseVM
                        {
                            CoursId = c.id,
                            Nom = c.Nom,
                            Niveau = c.Niveau,
                            Prix = c.Prix,
                            ImageUrl = c.ImageUrl,
                            Description = c.Description,
                            DejaInscrit = misIds.Contains(c.id)
                        })
                        .ToList();

            return View(vms);
        }

        // ---------------------- S'INSCRIRE (via microservice) ----------------------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Enroll(int coursId)
        {
            var uid = CurrentUserId();
            if (uid == null || !IsEtudiant())
                return RedirectToAction("Login", "Auth");

            // Vérifier localement si déjà inscrit (plus rapide)
            if (db.Inscriptions.Any(i => i.UtilisateurId == uid.Value && i.CoursId == coursId))
            {
                TempData["err"] = "Vous êtes déjà inscrit à ce cours.";
                return RedirectToAction("Browse");
            }

            // ----- 1) Appeler le microservice inscription_service (FastAPI) -----
            var payload = new
            {
                UtilisateurId = uid.Value,
                CoursId = coursId
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var resp = await httpInscription.PostAsync("inscriptions/enroll", content);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["err"] = "Erreur microservice d’inscription (code " + (int)resp.StatusCode + ").";
                    return RedirectToAction("Browse");
                }
                // Si quieres, podrías leer el JSON devuelto:
                // var body = await resp.Content.ReadAsStringAsync();
                // pero no es obligatorio para la logique MVC.
            }
            catch (Exception ex)
            {
                TempData["err"] = "Microservice d’inscription indisponible : " + ex.Message;
                return RedirectToAction("Browse");
            }

            // ----- 2) Regarder le prix du cours pour Stripe (BD locale) -----
            var prix = db.Cours
                         .Where(c => c.id == coursId)
                         .Select(c => c.Prix)
                         .FirstOrDefault();

            if (prix > 0)
            {
                // cours payant -> on va vers Stripe (microservice paiement)
                TempData["ok"] = "Inscription réussie. Redirection vers le paiement Stripe.";
                return RedirectToAction("Payer", "Paiement", new { id = coursId });
            }
            else
            {
                // cours gratuit -> rester dans l’appli
                TempData["ok"] = "Inscription réussie (cours gratuit).";
                return RedirectToAction("Index");
            }
        }

        // ---------------------- SE DÉSINSCRIRE ----------------------
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Unenroll(int coursId)
        {
            var uid = CurrentUserId();
            if (uid == null || !IsEtudiant())
                return RedirectToAction("Login", "Auth");

            var ins = db.Inscriptions
                        .FirstOrDefault(i => i.UtilisateurId == uid.Value && i.CoursId == coursId);

            if (ins != null)
            {
                db.Inscriptions.Remove(ins);
                db.SaveChanges();
                TempData["ok"] = "Inscription supprimée.";
            }

            return RedirectToAction("Browse");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
