using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    public class RessourceController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: /Ressource
        public ActionResult Index()
        {
      
            var ressources = db.Ressources.Include(r => r.ModuleCours).Include(r => r.ModuleCours.Cours).OrderBy(r => r.ModuleCours.Cours.Nom)
                .ThenBy(r => r.ModuleCours.Numero).ThenBy(r => r.Titre) .ToList();

            return View(ressources);
        }

        // GET: /Ressource/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);




            var ressource = db.Ressources.Include(r => r.ModuleCours).Include(r => r.ModuleCours.Cours).FirstOrDefault(r => r.id == id);

          if (ressource == null) return HttpNotFound();

            return View(ressource);
        }

        // GET: /Ressource/Create
        public ActionResult Create()
        {
            
            var modules = db.ModulesCours.Include(m => m.Cours).ToList();


            var options = modules.Select(m => new
            {
                id = m.id,

                texte = (m.Cours != null ? m.Cours.Nom : "Cours ?") + " - Module " + m.Numero
            });

            ViewBag.ModuleCoursId = new SelectList(options, "id", "texte");
            return View();
        }

        // POST: /Ressource/Create


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,ModuleCoursId,Type,Titre,Url")] Ressource r)
        {
         
            if (ModelState.IsValid)
            {
                db.Ressources.Add(r);  
                db.SaveChanges();
                TempData["ok"] = "Ressource créée avec succès.";
                return RedirectToAction("Index");
            }

     
            var modules = db.ModulesCours.Include(m => m.Cours).ToList();


            var options = modules.Select(m => new
            {
                id = m.id,
                texte = (m.Cours != null ? m.Cours.Nom : "Cours ?") + " - Module " + m.Numero
            });

            ViewBag.ModuleCoursId = new SelectList(options, "id", "texte", r.ModuleCoursId);
            return View(r);
        }

      
    }
}
