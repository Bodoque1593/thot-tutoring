using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    public class ModuleCoursController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var modules = db.ModulesCours
                            .Include(m => m.Cours)
                            .OrderBy(m => m.Cours.Nom).ThenBy(m => m.Numero).ToList();
            return View(modules);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var module = db.ModulesCours.Include(m => m.Cours).FirstOrDefault(m => m.id == id);
            return module == null ? (ActionResult)HttpNotFound() : View(module);
        }

        public ActionResult Create()
        {
            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom");
            ViewBag.UiHelp = true; return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,CoursId,Numero,Titre")] ModuleCours m)
        {
            if (db.ModulesCours.Any(x => x.CoursId == m.CoursId && x.Numero == m.Numero))
                ModelState.AddModelError("Numero", "Ce numéro existe déjà pour ce cours.");

            if (!ModelState.IsValid)
            { ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", m.CoursId); ViewBag.UiHelp = true; return View(m); }

            db.ModulesCours.Add(m); db.SaveChanges();
            TempData["ok"] = "Module créé avec succès. Ajoutez maintenant sa première ressource.";
            return RedirectToAction("Create", "Ressource", new { moduleId = m.id });
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var module = db.ModulesCours.Find(id); if (module == null) return HttpNotFound();
            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", module.CoursId);
            return View(module);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,CoursId,Numero,Titre")] ModuleCours m)
        {
            if (db.ModulesCours.Any(x => x.CoursId == m.CoursId && x.Numero == m.Numero && x.id != m.id))
                ModelState.AddModelError("Numero", "Ce numéro existe déjà pour ce cours.");

            if (!ModelState.IsValid)
            { ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", m.CoursId); return View(m); }

            db.Entry(m).State = EntityState.Modified; db.SaveChanges();
            TempData["ok"] = "Module modifié."; return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var module = db.ModulesCours.Include(m => m.Cours).FirstOrDefault(m => m.id == id);
            return module == null ? (ActionResult)HttpNotFound() : View(module);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var module = db.ModulesCours.Find(id);
            if (module != null) { db.ModulesCours.Remove(module); db.SaveChanges(); TempData["ok"] = "Module supprimé."; }
            return RedirectToAction("Index");
        }
    }
}
