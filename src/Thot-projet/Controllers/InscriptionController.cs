using System;
using System.Data.Entity;
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

        private bool IsEtudiant()
            => string.Equals(Convert.ToString(Session["UserRole"]), "Etudiant", StringComparison.OrdinalIgnoreCase);

        private int? CurrentUserId()
        {
            return Session["UserId"] as int?;
        }

        /// GET: /Inscription/Browse
        [HttpGet]
        public ActionResult Browse()
        {
            // Solo estudiantes autenticados
            var uid = CurrentUserId();
            if (uid == null || !IsEtudiant())
                return RedirectToAction("Login", "Auth");

            var myCourseIds = db.Inscriptions
                                .Where(i => i.UtilisateurId == uid.Value)
                                .Select(i => i.CoursId)
                                .ToList();

            var vms = db.Cours
                .Select(c => new InscriptionBrowseVM
                {
                    CoursId = c.id,
                    Nom = c.Nom,
                    Niveau = c.Niveau,
                    DejaInscrit = myCourseIds.Contains(c.id)
                })
                .OrderBy(x => x.Nom)
                .ToList();

            return View(vms); // Views/Inscription/Browse.cshtml
        }

        /// POST: /Inscription/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(int coursId)
        {
            var uid = CurrentUserId();
            if (uid == null || !IsEtudiant())
                return RedirectToAction("Login", "Auth");

            bool exists = db.Inscriptions.Any(i => i.UtilisateurId == uid.Value && i.CoursId == coursId);
            if (!exists)
            {
                db.Inscriptions.Add(new Inscription
                {
                    UtilisateurId = uid.Value,
                    CoursId = coursId,
                    InscritLe = DateTime.Now
                });
                db.SaveChanges();
                TempData["ok"] = "Inscription réussie.";
            }
            else
            {
                TempData["err"] = "Vous êtes déjà inscrit à ce cours.";
            }

            // Si prefieres llevarlo a Mes cours:
            return RedirectToAction("Index"); // tu vista de "Mes cours"
                                              // O, si luego conectas pago: return RedirectToAction("Create","Paiement", new { coursId });
        }
    }
    }
