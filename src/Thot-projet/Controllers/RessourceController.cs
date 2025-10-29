using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Tuteur")]
    public class RessourceController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var list = db.Ressources
                         .Include(r => r.ModuleCours)
                         .Include(r => r.ModuleCours.Cours)
                         .OrderBy(r => r.ModuleCours.Cours.Nom)
                         .ThenBy(r => r.ModuleCours.Numero)
                         .ToList();
            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.ModuleCoursId = new SelectList(
                db.ModulesCours
                  .Include(m => m.Cours)
                  .OrderBy(m => m.Cours.Nom)
                  .ThenBy(m => m.Numero)
                  .ToList()
                  .Select(m => new { m.id, Etiq = m.Cours.Nom + " - Module " + m.Numero }),
                "id", "Etiq");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ModuleCoursId,Type,Titre,url")] Ressource r)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ModuleCoursId = new SelectList(
                    db.ModulesCours.Include(m => m.Cours).ToList()
                      .Select(m => new { m.id, Etiq = m.Cours.Nom + " - Module " + m.Numero }),
                    "id", "Etiq", r.ModuleCoursId);
                return View(r);
            }

            db.Ressources.Add(r);
            db.SaveChanges();
            TempData["ok"] = "Ressource créée.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var r = db.Ressources.Find(id);
            if (r == null) return HttpNotFound();

            ViewBag.ModuleCoursId = new SelectList(
                db.ModulesCours.Include(m => m.Cours).ToList()
                  .Select(m => new { m.id, Etiq = m.Cours.Nom + " - Module " + m.Numero }),
                "id", "Etiq", r.ModuleCoursId);

            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,ModuleCoursId,Type,Titre,url")] Ressource r)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ModuleCoursId = new SelectList(
                    db.ModulesCours.Include(m => m.Cours).ToList()
                      .Select(m => new { m.id, Etiq = m.Cours.Nom + " - Module " + m.Numero }),
                    "id", "Etiq", r.ModuleCoursId);
                return View(r);
            }

            db.Entry(r).State = EntityState.Modified;
            db.SaveChanges();
            TempData["ok"] = "Ressource modifiée.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var r = db.Ressources
                      .Include(x => x.ModuleCours.Cours)
                      .FirstOrDefault(x => x.id == id);
            if (r == null) return HttpNotFound();
            return View(r);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var r = db.Ressources.Find(id);
            if (r != null)
            {
                db.Ressources.Remove(r);
                db.SaveChanges();
                TempData["ok"] = "Ressource supprimée.";
            }
            return RedirectToAction("Index");
        }

   
    }
}
