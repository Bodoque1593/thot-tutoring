using System;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Etudiant", "Tuteur")]
    public class EntreeFAQController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var list = db.EntreesFAQ.OrderByDescending(f => f.PublieLe).ToList();
            return View(list);
        }

        [RoleAuthorize("Tuteur")]
        public ActionResult Create() => View(new EntreeFAQ { PublieLe = DateTime.UtcNow });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult Create(EntreeFAQ m)
        {
            if (string.IsNullOrWhiteSpace(m.QuestionTexte))
                ModelState.AddModelError("QuestionTexte", "La question est requise.");
            if (string.IsNullOrWhiteSpace(m.ReponseTexte))
                ModelState.AddModelError("ReponseTexte", "La réponse est requise.");

            if (!ModelState.IsValid) return View(m);

            m.PublieLe = DateTime.UtcNow;
            db.EntreesFAQ.Add(m);
            db.SaveChanges();
            TempData["ok"] = "FAQ ajoutée.";
            return RedirectToAction("Index");
        }

        [RoleAuthorize("Tuteur")]
        public ActionResult Edit(int id)
        {
            var e = db.EntreesFAQ.Find(id);
            if (e == null) return HttpNotFound();
            return View(e);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult Edit(EntreeFAQ m)
        {
            if (!ModelState.IsValid) return View(m);
            var e = db.EntreesFAQ.Find(m.id);
            if (e == null) return HttpNotFound();

            e.QuestionTexte = m.QuestionTexte;
            e.ReponseTexte = m.ReponseTexte;
            db.SaveChanges();
            TempData["ok"] = "FAQ modifiée.";
            return RedirectToAction("Index");
        }

        [RoleAuthorize("Tuteur")]
        public ActionResult Delete(int id)
        {
            var e = db.EntreesFAQ.Find(id);
            if (e == null) return HttpNotFound();
            return View(e);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var e = db.EntreesFAQ.Find(id);
            if (e != null)
            {
                db.EntreesFAQ.Remove(e);
                db.SaveChanges();
                TempData["ok"] = "FAQ supprimée.";
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
