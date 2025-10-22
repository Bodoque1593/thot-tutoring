using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
   
    [RoleAuthorize("Etudiant")]
    public class InscriptionController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        
        public ActionResult Index()
        {
            int uid = (int)(Session["UserId"] ?? 0);

            var mes = db.Inscriptions.Include(i => i.Cours).Where(i => i.UtilisateurId == uid).OrderByDescending(i => i.InscritLe).ToList();
            return View(mes);
        }

        
        public ActionResult Browse()
        {
            int uid = (int)(Session["UserId"] ?? 0);

            var inscrits = db.Inscriptions.Where(i => i.UtilisateurId == uid).Select(i => i.CoursId).ToList();

            var vm = db.Cours.OrderBy(c => c.Nom).Select(c => new BrowseCoursVM
                       {CoursId = c.id,Nom = c.Nom, Niveau = c.Niveau,DejaInscrit = inscrits.Contains(c.id)}).ToList();

            return View(vm);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(int coursId)
        {
            int uid = (int)(Session["UserId"] ?? 0);

            // inscrito
            bool existe = db.Inscriptions.Any(i => i.UtilisateurId == uid && i.CoursId == coursId);
            if (existe)
            {
                TempData["err"] = "Vous êtes déjà inscrit à ce cours.";
                return RedirectToAction("Browse");
            }

            
            var cours = db.Cours.Find(coursId);
            if (cours == null)
            {
                TempData["err"] = "Cours introuvable.";
                return RedirectToAction("Browse");
            }

            var ins = new Inscription
            {
                UtilisateurId = uid,CoursId = coursId,InscritLe = DateTime.UtcNow
            };

            db.Inscriptions.Add(ins);
            db.SaveChanges();

            TempData["ok"] = "Inscription réussie.";
            return RedirectToAction("Index");
        }

     
    }

    // VM local para Browse
    public class BrowseCoursVM
    {
        public int CoursId { get; set; }
        public string Nom { get; set; }
        public string Niveau { get; set; }
        public bool DejaInscrit { get; set; }
    }
}
