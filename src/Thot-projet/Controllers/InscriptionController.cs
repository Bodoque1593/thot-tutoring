using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    public class InscriptionController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: /Inscription
        
        public ActionResult Index()
        {
            var list = db.Inscriptions
                         .Include(i => i.Utilisateur)
                         .Include(i => i.Cours)
                         .OrderBy(i => i.Cours.Nom)
                         .ThenBy(i => i.Utilisateur.Nomcomplet)
                         .ToList();
            return View(list);
        }

        // GET: /Inscription/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = db.Inscriptions
                         .Include(i => i.Utilisateur)
                         .Include(i => i.Cours)
                         .FirstOrDefault(i => i.id == id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        // GET: /Inscription/Create
     
        public ActionResult Create()
        {
            
            var etudiants = db.Utilisateurs
                              .Where(u => u.Role == "Etudiant")
                              .OrderBy(u => u.Nomcomplet)
                              .Select(u => new { u.id, u.Nomcomplet })
                              .ToList();

            ViewBag.UtilisateurId = new SelectList(etudiants, "id", "NomComplet");
            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom");
            return View();
        }

        // POST: /Inscription/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,UtilisateurId,CoursId")] Inscription i)
        {
         
            bool existe = db.Inscriptions.Any(x => x.UtilisateurId == i.UtilisateurId && x.CoursId == i.CoursId);
            if (existe)
            {
                ModelState.AddModelError("", "Cette inscription existe déjà pour cet étudiant et ce cours.");
            }

            if (ModelState.IsValid)
            {
                i.InscritLe = DateTime.Now; // date d’inscription
                db.Inscriptions.Add(i);
                db.SaveChanges();
                TempData["ok"] = "Inscription créée avec succès.";
                return RedirectToAction("Index");
            }

            // Recharger les listes si la validation échoue
            var etudiants = db.Utilisateurs
                              .Where(u => u.Role == "Etudiant")
                              .OrderBy(u => u.Nomcomplet)
                              .Select(u => new { u.id, u.Nomcomplet })
                              .ToList();

            ViewBag.UtilisateurId = new SelectList(etudiants, "id", "NomComplet", i.UtilisateurId);
            ViewBag.CoursId = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", i.CoursId);
            return View(i);
        }

        // GET: /Inscription/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var item = db.Inscriptions
                         .Include(x => x.Utilisateur)
                         .Include(x => x.Cours)
                         .FirstOrDefault(x => x.id == id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        // POST: /Inscription/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var item = db.Inscriptions.Find(id);
            if (item != null)
            {
                db.Inscriptions.Remove(item);
                db.SaveChanges();
                TempData["ok"] = "Inscription supprimée.";
            }
            return RedirectToAction("Index");
        }

   
    }
}
