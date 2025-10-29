using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Etudiant", "Tuteur")]
    public class QuestionController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // LISTA:
        // - Tuteur: todas no resueltas (cola)
        // - Etudiant: solo mías
        public ActionResult Index()
        {
            int uid = (int)(Session["UserId"] ?? 0);
            string role = Convert.ToString(Session["UserRole"] ?? "");

            if (string.Equals(role, "Tuteur", StringComparison.OrdinalIgnoreCase))
            {
                var ouvertes = db.Questions
                    .Include(q => q.Cours).Include(q => q.Ressource).Include(q => q.Etudiant)
                    .Where(q => !q.EstResolvee)
                    .OrderByDescending(q => q.Creele)
                    .ToList();
                return View("Index_Tuteur", ouvertes);
            }
            else
            {
                var miennes = db.Questions
                    .Include(q => q.Cours).Include(q => q.Ressource)
                    .Where(q => q.EtudiantId == uid)
                    .OrderByDescending(q => q.Creele)
                    .ToList();
                return View("Index_Etudiant", miennes);
            }
        }

        public ActionResult Details(int id)
        {
            var q = db.Questions
                .Include(x => x.Cours)
                .Include(x => x.Ressource)
                .Include(x => x.Etudiant)
                .Include(x => x.Reponses.Select(r => r.Tuteur))
                .FirstOrDefault(x => x.id == id);

            if (q == null) return HttpNotFound();
            return View(q);
        }

        // ---------- ETUDIANT: CREAR PREGUNTA ----------
        [RoleAuthorize("Etudiant")]
        public ActionResult Create()
        {
            int uid = (int)(Session["UserId"] ?? 0);
            var coursIds = db.Inscriptions.Where(i => i.UtilisateurId == uid).Select(i => i.CoursId).ToList();

            ViewBag.Cours = db.Cours
                .Where(c => coursIds.Contains(c.id))
                .OrderBy(c => c.Nom)
                .Select(c => new SelectListItem { Value = c.id.ToString(), Text = c.Nom })
                .ToList();

            ViewBag.Ressources = Enumerable.Empty<SelectListItem>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Etudiant")]
        public ActionResult Create(int CoursId, int RessourceId, string Contenu)
        {
            int uid = (int)(Session["UserId"] ?? 0);

            if (!db.Inscriptions.Any(i => i.UtilisateurId == uid && i.CoursId == CoursId))
                ModelState.AddModelError("CoursId", "Vous devez être inscrit à ce cours.");

            if (!db.Ressources.Any(r => r.id == RessourceId && r.ModuleCours.CoursId == CoursId))
                ModelState.AddModelError("RessourceId", "Ressource invalide pour ce cours.");

            if (string.IsNullOrWhiteSpace(Contenu))
                ModelState.AddModelError("Contenu", "Le contenu est requis.");

            if (!ModelState.IsValid)
            {
                var coursIds = db.Inscriptions.Where(i => i.UtilisateurId == uid).Select(i => i.CoursId).ToList();
                ViewBag.Cours = db.Cours.Where(c => coursIds.Contains(c.id))
                    .OrderBy(c => c.Nom).Select(c => new SelectListItem { Value = c.id.ToString(), Text = c.Nom }).ToList();

                ViewBag.Ressources = db.Ressources.Where(r => r.ModuleCours.CoursId == CoursId)
                    .OrderBy(r => r.Titre).Select(r => new SelectListItem { Value = r.id.ToString(), Text = r.Titre }).ToList();

                ViewBag.CoursIdSel = CoursId;
                return View();
            }

            var q = new Question
            {
                EtudiantId = uid,
                CoursId = CoursId,
                RessourceId = RessourceId,
                Contenu = Contenu.Trim(),
                EstResolvee = false,
                Creele = DateTime.UtcNow
            };
            db.Questions.Add(q);
            db.SaveChanges();

            TempData["ok"] = "Votre question a été créée.";
            return RedirectToAction("Index");
        }

        // AJAX: recursos por curso
        [HttpGet]
        [RoleAuthorize("Etudiant")]
        public ActionResult RessourcesParCours(int coursId)
        {
            var items = db.Ressources
                .Where(r => r.ModuleCours.CoursId == coursId)
                .OrderBy(r => r.Titre)
                .Select(r => new { r.id, r.Titre })
                .ToList();
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        // ---------- TUTEUR: RESPONDER / RESOLVER ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult Repondre(int questionId, string contenu)
        {
            int uid = (int)(Session["UserId"] ?? 0);
            var q = db.Questions.FirstOrDefault(x => x.id == questionId);
            if (q == null) { TempData["err"] = "Question introuvable."; return RedirectToAction("Index"); }

            if (string.IsNullOrWhiteSpace(contenu))
            {
                TempData["err"] = "Le contenu de la réponse est requis.";
                return RedirectToAction("Details", new { id = questionId });
            }

            var rep = new Reponse
            {
                QuestionId = questionId,
                TuteurId = uid,
                Contenu = contenu.Trim(),
                Creele = DateTime.UtcNow
            };
            db.Reponses.Add(rep);
            db.SaveChanges();

            TempData["ok"] = "Réponse ajoutée.";
            return RedirectToAction("Details", new { id = questionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult MarquerResolue(int questionId)
        {
            var q = db.Questions.FirstOrDefault(x => x.id == questionId);
            if (q == null) { TempData["err"] = "Question introuvable."; return RedirectToAction("Index"); }

            q.EstResolvee = true;
            db.SaveChanges();

            TempData["ok"] = "Question marquée comme résolue.";
            return RedirectToAction("Details", new { id = questionId });
        }

       
    }
}
