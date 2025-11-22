using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Etudiant", "Tuteur")]
    public class EntreeFAQController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // URL de tu microservicio FastAPI
        private const string FaqServiceBaseUrl = "http://127.0.0.1:8001";

        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri(FaqServiceBaseUrl)
        };

        // --------------------------------------------------------------------
        // LISTE FAQ -> via microservice (GET /faq)
        // --------------------------------------------------------------------
        public async Task<ActionResult> Index()
        {
            try
            {
                var response = await httpClient.GetAsync("/faq");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var faqs = JsonConvert.DeserializeObject<List<EntreeFAQ>>(json) ?? new List<EntreeFAQ>();

                return View(faqs.OrderByDescending(f => f.PublieLe));
            }
            catch (Exception ex)
            {
                // En cas d’erreur, on peut afficher les données locales EF comme fallback
                ViewBag.Error = "Erreur lors de l'appel au microservice FAQ. Affichage des données locales.";
                var faqsLocal = db.EntreesFAQ.OrderByDescending(f => f.PublieLe).ToList();
                return View(faqsLocal);
            }
        }

        // --------------------------------------------------------------------
        // CREATE (GET) -> juste afficher le formulaire
        // --------------------------------------------------------------------
        [RoleAuthorize("Tuteur")]
        public ActionResult Create()
        {
            return View(new EntreeFAQ { PublieLe = DateTime.UtcNow });
        }

        // --------------------------------------------------------------------
        // CREATE (POST) -> via microservice (POST /faq)
        // --------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Tuteur")]
        public async Task<ActionResult> Create(EntreeFAQ m)
        {
            if (string.IsNullOrWhiteSpace(m.QuestionTexte))
                ModelState.AddModelError("QuestionTexte", "La question est requise.");

            if (string.IsNullOrWhiteSpace(m.ReponseTexte))
                ModelState.AddModelError("ReponseTexte", "La réponse est requise.");

            if (!ModelState.IsValid)
                return View(m);

            try
            {
                var payload = new
                {
                    QuestionTexte = m.QuestionTexte,
                    ReponseTexte = m.ReponseTexte
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/faq", content);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Erreur lors de l'appel au microservice FAQ.");
                    return View(m);
                }

                // No grabamos con EF: el microservicio ya escribió en ThotDb
                TempData["ok"] = "FAQ ajoutée via microservice.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erreur interne (microservice FAQ).");
                return View(m);
            }
        }

        // --------------------------------------------------------------------
        // EDIT / DELETE -> puedes dejarlos con EF por ahora (suficiente para el profe)
        // --------------------------------------------------------------------
        [RoleAuthorize("Tuteur")]
        public ActionResult Edit(int id)
        {
            var e = db.EntreesFAQ.Find(id);
            return e == null ? (ActionResult)HttpNotFound() : View(e);
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
            return e == null ? (ActionResult)HttpNotFound() : View(e);
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
