using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
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

        // GET: /Question/
        public ActionResult Index()
        {
            var questions = db.Questions
                .Include(q => q.Cours)
                .Include(q => q.Ressource)
                .Include(q => q.Etudiant)
                .OrderByDescending(q => q.Creele)
                .ToList();

            return View(questions);
        }

        // GET: /Question/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var q = db.Questions
                .Include(x => x.Cours)
                .Include(x => x.Ressource)
                .Include(x => x.Reponses.Select(r => r.Tuteur))
                .FirstOrDefault(x => x.id == id);

            if (q == null) return HttpNotFound();

            return View(q);
        }

        // GET: /Question/Create
        [RoleAuthorize("Etudiant")]
        public ActionResult Create(int? coursId = null, int? ressourceId = null)
        {
            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", coursId);
            ViewBag.RessourceId = new SelectList(db.Ressources.OrderBy(r => r.Titre), "id", "Titre", ressourceId);
            return View();
        }

        // POST: /Question/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Etudiant")]
        public ActionResult Create([Bind(Include = "CoursId,RessourceId,Contenu")] Question question)
        {
            int? uid = Session["UserId"] as int?;
            if (uid == null)
                return RedirectToAction("Login", "Auth");

            // Si no hay RessourceId, permitimos null (pregunta general)
            if (question.RessourceId == 0)
                question.RessourceId = null;

            if (!ModelState.IsValid)
            {
                ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", question.CoursId);
                ViewBag.RessourceId = new SelectList(db.Ressources.OrderBy(r => r.Titre), "id", "Titre", question.RessourceId);
                return View(question);
            }

            question.EtudiantId = uid.Value;
            question.Creele = DateTime.Now;
            question.EstResolvee = false;

            db.Questions.Add(question);
            db.SaveChanges();

            TempData["ok"] = "Question envoyée avec succès.";
            return RedirectToAction("Index");
        }

        // GET: /Question/Answer/5  (Tuteur répond)
        [RoleAuthorize("Tuteur")]
        public ActionResult Answer(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var q = db.Questions.Find(id);
            if (q == null) return HttpNotFound();

            ViewBag.Question = q;
            return View();
        }

        // POST: /Question/Answer
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult Answer(int questionId, string contenu)
        {
            int? uid = Session["UserId"] as int?;
            if (uid == null)
                return RedirectToAction("Login", "Auth");

            var q = db.Questions.Find(questionId);
            if (q == null)
            {
                TempData["err"] = "Question introuvable.";
                return RedirectToAction("Index");
            }

            var r = new Reponse
            {
                QuestionId = q.id,
                TuteurId = uid.Value,
                Contenu = contenu,
                Creele = DateTime.Now
            };

            db.Reponses.Add(r);
            q.EstResolvee = true;
            db.SaveChanges();

            TempData["ok"] = "Réponse envoyée.";
            return RedirectToAction("Details", new { id = q.id });
        }

        // DELETE (solo para Tuteur)
        [RoleAuthorize("Tuteur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var q = db.Questions.Include(x => x.Cours).FirstOrDefault(x => x.id == id);
            if (q == null) return HttpNotFound();

            return View(q);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var q = db.Questions
                .Include(x => x.Reponses)
                .FirstOrDefault(x => x.id == id);

            if (q != null)
            {
                db.Reponses.RemoveRange(q.Reponses.ToList());
                db.Questions.Remove(q);
                db.SaveChanges();
                TempData["ok"] = "Question supprimée.";
            }
            return RedirectToAction("Index");
        }

  
    }
}
