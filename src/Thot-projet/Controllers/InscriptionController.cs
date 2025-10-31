using System;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Etudiant")]
    public class InscriptionController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private int? CurrentUserId() => Session["UserId"] as int?;
        private bool IsEtudiant()
            => string.Equals(Convert.ToString(Session["UserRole"]), "Etudiant", StringComparison.OrdinalIgnoreCase);

        // GET: /Inscription  (Mes cours)
        public ActionResult Index()
        {
            var uid = CurrentUserId();
            if (uid == null) return RedirectToAction("Login", "Auth");

            var list = db.Inscriptions
                         .Where(i => i.UtilisateurId == uid.Value)
                         .OrderByDescending(i => i.InscritLe)
                         .ToList();

            return View(list);
        }

        // GET: /Inscription/Browse  (catálogo de cursos para inscribirse)
        [HttpGet]
        public ActionResult Browse()
        {
            var uid = CurrentUserId();
            if (uid == null || !IsEtudiant()) return RedirectToAction("Login", "Auth");

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

            return View(vms); // Views/Inscription/Browse.cshtml
        }

        // POST: /Inscription/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(int coursId)
        {
            var uid = CurrentUserId();
            if (uid == null || !IsEtudiant()) return RedirectToAction("Login", "Auth");

            var yaExiste = db.Inscriptions.Any(i => i.UtilisateurId == uid.Value && i.CoursId == coursId);
            if (yaExiste)
            {
                TempData["err"] = "Vous êtes déjà inscrit à ce cours.";
                return RedirectToAction("Index");
            }

            db.Inscriptions.Add(new Inscription
            {
                UtilisateurId = uid.Value,
                CoursId = coursId,
                InscritLe = DateTime.UtcNow
            });
            db.SaveChanges();

            TempData["ok"] = "Inscription réussie.";

            // flujo de pago si el curso tiene precio (> 0)
            var prix = db.Cours.Where(c => c.id == coursId)
                               .Select(c => c.Prix)
                               .FirstOrDefault();

            if (prix > 0)
            {
                // la action Create del PaiementController admite: Create(decimal? Montant, string Monnaie, string Statut)
                return RedirectToAction(
                    "Create",
                    "Paiement",
                    new { Montant = prix, Monnaie = "CAD", Statut = "Payé" }
                );
            }

            return RedirectToAction("Index");
        }

        // POST: /Inscription/Unenroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unenroll(int coursId)
        {
            var uid = CurrentUserId();
            if (uid == null || !IsEtudiant()) return RedirectToAction("Login", "Auth");

            var ins = db.Inscriptions.FirstOrDefault(i => i.UtilisateurId == uid.Value && i.CoursId == coursId);
            if (ins != null)
            {
                db.Inscriptions.Remove(ins);
                db.SaveChanges();
                TempData["ok"] = "Inscription supprimée.";
            }
            return RedirectToAction("Browse");
        }
    }
}
