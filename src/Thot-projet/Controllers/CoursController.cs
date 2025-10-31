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

        // GET: Cours
        public ActionResult Index()
        {
            var cours = db.Cours.OrderBy(c => c.Nom).ToList();
            return View(cours);
        }

        [RoleAuthorize("Tuteur")]
        public ActionResult Create()
        {
            // valores por defecto opcionales
            var model = new Cours { Prix = 0m };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult Create([Bind(Include = "Nom,Niveau,Prix,ImageUrl,Description")] Cours cours)
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

        [RoleAuthorize("Tuteur")]
        public ActionResult Vitrine()
        {
            var list = db.Cours.OrderBy(c => c.Nom).ToList();
            return View(list);
        }


        [RoleAuthorize("Etudiant", "Tuteur")]
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var c = db.Cours
                .Include(x => x.Modules.Select(m => m.Ressources))
                .Include(x => x.Questions)
                .FirstOrDefault(x => x.id == id);

            if (c == null) return HttpNotFound();

            return View(c); // Ya tienes Views/Cours/Details.cshtml
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult Edit([Bind(Include = "id,Nom,Niveau,Prix,ImageUrl,Description")] Cours cours)
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





        // Borrado en cascada manual (si tu SQL no tiene ON DELETE CASCADE)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult DeleteConfirmed(int id)
        {
            // Cargar el curso con todas sus relaciones necesarias
            var cours = db.Cours
                .Include(c => c.Inscriptions)
                .Include(c => c.Modules.Select(m => m.Ressources))
                .Include(c => c.Questions.Select(q => q.Reponses))
                .FirstOrDefault(c => c.id == id);

            if (cours == null)
            {
                TempData["err"] = "Le cours est introuvable.";
                return RedirectToAction("Index");
            }

            try
            {
                // 1️⃣ Eliminar respuestas y preguntas relacionadas
                foreach (var q in cours.Questions.ToList())
                {
                    db.Reponses.RemoveRange(q.Reponses.ToList());
                    db.Questions.Remove(q);
                }

                // 2️⃣ Eliminar recursos y módulos
                foreach (var m in cours.Modules.ToList())
                {
                    db.Ressources.RemoveRange(m.Ressources.ToList());
                    db.ModulesCours.Remove(m);
                }

                // 3️⃣ Eliminar inscripciones
                db.Inscriptions.RemoveRange(cours.Inscriptions.ToList());

                // 4️⃣ Finalmente, eliminar el curso
                db.Cours.Remove(cours);
                db.SaveChanges();

                TempData["ok"] = "Cours supprimé avec succès.";
            }
            catch (Exception)
            {
                TempData["err"] = "Impossible de supprimer : le cours a des inscriptions, modules ou ressources.";
            }

            return RedirectToAction("Index");
        }

    }
}