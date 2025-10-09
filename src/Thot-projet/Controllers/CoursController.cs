using System.Linq;
using System.Net;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    public class CoursController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: /Cours
        public ActionResult Index()
        {
            var items = db.Cours.OrderBy(c => c.Nom).ToList();
            return View(items);
        }

        // GET: /Cours/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var cours = db.Cours.Find(id);


            if (cours == null) return HttpNotFound();
            return View(cours);
        }

        // GET: /Cours/Create
        public ActionResult Create() => View();// formulario vacio




        // POST: /Cours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,Nom,Niveau")] Cours cours) // llena el objeto cours con los datos del formulario
        {
            if (ModelState.IsValid)
            {
                db.Cours.Add(cours);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cours);
        }




        // GET: /Cours/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var cours = db.Cours.Find(id);
            if (cours == null) return HttpNotFound();
            return View(cours); //precarga el objeto
        }

        // POST: /Cours/Edit/5


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,Nom,Niveau")] Cours cours)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cours).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cours);
        }

        // GET: /Cours/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var cours = db.Cours.Find(id);
            if (cours == null) return HttpNotFound();
            return View(cours); // muestra confirmacion
        }


        // POST: /Cours/Delete/5
        [HttpPost, ActionName("Delete")]    // el metodo es DeleteConfirmed pero la ruta post es Delete  


        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)   
        {
            var cours = db.Cours.Find(id);
            if (cours != null)
            {
                db.Cours.Remove(cours);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose(); // cierra conexiones a sql
            base.Dispose(disposing);
        }
    }
}
