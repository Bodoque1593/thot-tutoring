using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Tuteur", "Etudiant")]
    public class CoursController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // HttpClient pour parler au microservice vitrine (FastAPI)
        private static readonly HttpClient httpVitrine = new HttpClient
        {
            // ⚠️ IMPORTANTE: este puerto debe ser el MISMO que uses en uvicorn
            BaseAddress = new Uri("http://127.0.0.1:8006/")
        };

        // Liste simple des cours (EF direct, pour la gestion interne)
        public ActionResult Index()
            => View(db.Cours.OrderBy(c => c.Nom).ToList());

        // Carte vitrine (Tuteur) -> essaie microservice, sinon fallback EF
        [RoleAuthorize("Tuteur")]
        public async Task<ActionResult> Vitrine()
        {
            try
            {
                // Llama a GET http://127.0.0.1:8006/courses
                var resp = await httpVitrine.GetAsync("courses");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<Cours>>(json);

                    if (list != null)
                        return View(list);
                }
                else
                {
                    TempData["err"] = "Microservice vitrine : code " + (int)resp.StatusCode;
                }
            }
            catch (Exception ex)
            {
                TempData["err"] = "Microservice vitrine indisponible : " + ex.Message;
            }

            // Fallback : si el microservicio falla, seguimos mostrando EF normal
            var fallback = db.Cours.OrderBy(c => c.Nom).ToList();
            return View(fallback);
        }

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
        public ActionResult Create()
            => View(new Cours { Prix = 0m });

        [HttpPost, ValidateAntiForgeryToken, RoleAuthorize("Tuteur")]
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
            return c == null ? (ActionResult)HttpNotFound() : View(c);
        }

        [HttpPost, ValidateAntiForgeryToken, RoleAuthorize("Tuteur")]
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
            return c == null ? (ActionResult)HttpNotFound() : View(c);
        }

        // Suppression manuelle en cascade (ta logique d’origine)
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, RoleAuthorize("Tuteur")]
        public ActionResult DeleteConfirmed(int id)
        {
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
                foreach (var q in cours.Questions.ToList())
                {
                    db.Reponses.RemoveRange(q.Reponses.ToList());
                    db.Questions.Remove(q);
                }

                foreach (var m in cours.Modules.ToList())
                {
                    db.Ressources.RemoveRange(m.Ressources.ToList());
                    db.ModulesCours.Remove(m);
                }

                db.Inscriptions.RemoveRange(cours.Inscriptions.ToList());
                db.Cours.Remove(cours);
                db.SaveChanges();
                TempData["ok"] = "Cours supprimé avec succès.";
            }
            catch (Exception)
            {
                TempData["err"] = "Impossible de supprimer : éléments associés.";
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
