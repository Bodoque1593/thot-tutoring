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
    [RoleAuthorize("Tuteur", "Etudiant")]
    public class CoursController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Liste simple des cours
        public ActionResult Index() => View(db.Cours.OrderBy(c => c.Nom).ToList());

        // Carte vitrine (lecture)
        [RoleAuthorize("Tuteur")]
        public ActionResult Vitrine() => View(db.Cours.OrderBy(c => c.Nom).ToList());

        // Détails (chargement navigation minimale utile aux vues)
        [RoleAuthorize("Etudiant", "Tuteur")]
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var c = db.Cours
                      .Include(x => x.Modules.Select(m => m.Ressources))
                      .Include(x => x.Questions)
                      .FirstOrDefault(x => x.id == id);
            return c == null ? (ActionResult)HttpNotFound() : View(c);
        }

        // ---- CRUD tuteur ----------------------------------------------------
        [RoleAuthorize("Tuteur")]
        public ActionResult Create() => View(new Cours { Prix = 0m });

        [HttpPost, ValidateAntiForgeryToken, RoleAuthorize("Tuteur")]
        public ActionResult Create([Bind(Include = "Nom,Niveau,Prix,ImageUrl,Description")] Cours cours)
        {
            if (!ModelState.IsValid) return View(cours);
            db.Cours.Add(cours); db.SaveChanges();
            TempData["ok"] = "Cours créé."; return RedirectToAction("Index");
        }

        [RoleAuthorize("Tuteur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var c = db.Cours.Find(id);
            return c == null ? (ActionResult)HttpNotFound() : View(c);
        }

        [HttpPost, ValidateAntiForgeryToken, RoleAuthorize("Tuteur")]
        public ActionResult Edit([Bind(Include = "id,Nom,Niveau,Prix,ImageUrl,Description")] Cours cours)
        {
            if (!ModelState.IsValid) return View(cours);
            db.Entry(cours).State = EntityState.Modified; db.SaveChanges();
            TempData["ok"] = "Cours modifié."; return RedirectToAction("Index");
        }

        [RoleAuthorize("Tuteur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var c = db.Cours.Find(id);
            return c == null ? (ActionResult)HttpNotFound() : View(c);
        }

        // Suppression manuelle en cascade (même logique que la tienne)
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, RoleAuthorize("Tuteur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var cours = db.Cours
                          .Include(c => c.Inscriptions)
                          .Include(c => c.Modules.Select(m => m.Ressources))
                          .Include(c => c.Questions.Select(q => q.Reponses))
                          .FirstOrDefault(c => c.id == id);

            if (cours == null) { TempData["err"] = "Le cours est introuvable."; return RedirectToAction("Index"); }

            try
            {
                foreach (var q in cours.Questions.ToList()) { db.Reponses.RemoveRange(q.Reponses.ToList()); db.Questions.Remove(q); }
                foreach (var m in cours.Modules.ToList()) { db.Ressources.RemoveRange(m.Ressources.ToList()); db.ModulesCours.Remove(m); }
                db.Inscriptions.RemoveRange(cours.Inscriptions.ToList());
                db.Cours.Remove(cours);
                db.SaveChanges();
                TempData["ok"] = "Cours supprimé avec succès.";
            }
            catch (Exception) { TempData["err"] = "Impossible de supprimer : éléments associés."; }

            return RedirectToAction("Index");
        }
    }
}
