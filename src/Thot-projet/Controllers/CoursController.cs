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

        // GET: Cours
        public ActionResult Index()
        {
            var cours = db.Cours.OrderBy(c => c.Nom).ToList();
            return View(cours);
        }

    
        [RoleAuthorize("Tuteur")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult Create([Bind(Include = "Nom,Niveau")] Cours cours)
        {
            if (!ModelState.IsValid) return View(cours);
            db.Cours.Add(cours);
            db.SaveChanges();
            TempData["ok"] = "Cours créé.";
            return RedirectToAction("Index");
        }

        [RoleAuthorize("Tuteur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var c = db.Cours.Find(id);
            if (c == null) return HttpNotFound();
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult Edit([Bind(Include = "id,Nom,Niveau")] Cours cours)
        {
            if (!ModelState.IsValid) return View(cours);
            db.Entry(cours).State = EntityState.Modified;
            db.SaveChanges();
            TempData["ok"] = "Cours modifié.";
            return RedirectToAction("Index");
        }

        [RoleAuthorize("Tuteur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var c = db.Cours.Find(id);
            if (c == null) return HttpNotFound();
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var c = db.Cours.Find(id);
            if (c != null)
            {
                db.Cours.Remove(c);
                db.SaveChanges();
                TempData["ok"] = "Cours supprimé.";
            }
            return RedirectToAction("Index");
        }

     
    }
}
