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

        // GET: /ModuleCours
        public ActionResult Index()
        {
            var modules = db.ModulesCours.Include(m => m.Cours).OrderBy(m => m.Cours.Nom)
                            .ThenBy(m => m.Numero)
                            .ToList();
            return View(modules);
        }

        // GET: /ModuleCours/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var module = db.ModulesCours.Include(m => m.Cours).FirstOrDefault(m => m.id == id);
            if (module == null) return HttpNotFound();
            return View(module);
        }

        public ActionResult Create()
        {
            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(

            [Bind(Include = "id,CoursId,Numero,Titre")] ModuleCours m
        )
        {
            if (ModelState.IsValid)
            {
                db.ModulesCours.Add(m);
                db.SaveChanges();
                TempData["ok"] = "Module créé avec succès.";
                return RedirectToAction("Index");
            }


            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", m.CoursId);
            return View(m);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var module = db.ModulesCours.Find(id);
            if (module == null) return HttpNotFound();

            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", module.CoursId);
            return View(module);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,CoursId,Numero,Titre")] ModuleCours m)
        {
            if (ModelState.IsValid)
            {
                db.Entry(m).State = EntityState.Modified; // UPDATE
                db.SaveChanges();
                TempData["ok"] = "Module modifié.";
                return RedirectToAction("Index");
            }
            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", m.CoursId);
            return View(m);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var module = db.ModulesCours.Include(m => m.Cours).FirstOrDefault(m => m.id == id);
            if (module == null) return HttpNotFound();
            return View(module);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var module = db.ModulesCours.Find(id);
            if (module != null)
            {
                db.ModulesCours.Remove(module);
                db.SaveChanges();
                TempData["ok"] = "Module supprimé.";
            }
            return RedirectToAction("Index");
        }

    }
}
