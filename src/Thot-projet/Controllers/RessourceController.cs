using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Tuteur")]
    public class RessourceController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        private const int MaxPdfBytes = 10 * 1024 * 1024;               // 10 MB
        private const string UploadFolderVirtual = "~/Content/uploads/resources";

        public ActionResult Index()
        {
            var data = db.Ressources
                         .Include(r => r.ModuleCours.Cours)
                         .OrderBy(r => r.ModuleCours.Cours.Nom)
                         .ThenBy(r => r.ModuleCours.Numero)
                         .ThenBy(r => r.Titre).ToList();
            return View(data);
        }

        public ActionResult Details(int id)
        {
            var r = db.Ressources.Include(x => x.ModuleCours.Cours).FirstOrDefault(x => x.id == id);
            return r == null ? (ActionResult)HttpNotFound() : View(r);
        }

        // ---- Create ---------------------------------------------------------
        [HttpGet]
        public ActionResult Create(int? moduleId = null)
        {
            ViewBag.ModuleCoursId = ModuleSelectList(moduleId);
            ViewBag.UiHelp = true; return View(new Ressource());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ModuleCoursId,Type,Titre,url")] Ressource r, HttpPostedFileBase fichier)
        {
            var uploaded = TrySavePdf(fichier);
            if (uploaded.Item1) { r.url = uploaded.Item2; if (string.IsNullOrWhiteSpace(r.Type)) r.Type = "PDF"; }

            if (string.IsNullOrWhiteSpace(r.url)) ModelState.AddModelError("url", "Indiquez un lien OU téléversez un PDF.");
            if (!ModelState.IsValid)
            {
                ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId); ViewBag.UiHelp = true;
                ViewBag.UploadError = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(r);
            }

            try { db.Ressources.Add(r); db.SaveChanges(); TempData["ok"] = "Ressource créée."; return RedirectToAction("Index"); }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors) ModelState.AddModelError(ve.PropertyName ?? "", ve.ErrorMessage);
                ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId); ViewBag.UiHelp = true; return View(r);
            }
        }

        // ---- Edit -----------------------------------------------------------
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var r = db.Ressources.Include(x => x.ModuleCours.Cours).FirstOrDefault(x => x.id == id);
            if (r == null) return HttpNotFound();
            ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId); return View(r);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,ModuleCoursId,Type,Titre,url")] Ressource r, HttpPostedFileBase fichier)
        {
            var uploaded = TrySavePdf(fichier);
            if (uploaded.Item1) { r.url = uploaded.Item2; if (string.IsNullOrWhiteSpace(r.Type)) r.Type = "PDF"; }

            if (string.IsNullOrWhiteSpace(r.url)) ModelState.AddModelError("url", "Indiquez un lien OU téléversez un PDF.");
            if (!ModelState.IsValid) { ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId); return View(r); }

            try { db.Entry(r).State = EntityState.Modified; db.SaveChanges(); TempData["ok"] = "Ressource modifiée."; return RedirectToAction("Index"); }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors) ModelState.AddModelError(ve.PropertyName ?? "", ve.ErrorMessage);
                ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId); return View(r);
            }
        }

        // ---- Delete ---------------------------------------------------------
        public ActionResult Delete(int id)
        {
            var r = db.Ressources.Include(x => x.ModuleCours.Cours).FirstOrDefault(x => x.id == id);
            return r == null ? (ActionResult)HttpNotFound() : View(r);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var r = db.Ressources.Find(id);
            if (r != null) { db.Ressources.Remove(r); db.SaveChanges(); TempData["ok"] = "Ressource supprimée."; }
            return RedirectToAction("Index");
        }

        // ---- Helpers --------------------------------------------------------
        private SelectList ModuleSelectList(int? selectedId = null)
        {
            var items = db.ModulesCours.Include(m => m.Cours)
                           .OrderBy(m => m.Cours.Nom).ThenBy(m => m.Numero)
                           .Select(m => new { m.id, Texte = (m.Cours.Nom + " – Module " + m.Numero + " : " + m.Titre) }).ToList();
            return new SelectList(items, "id", "Texte", selectedId);
        }

        // Sauvegarde un PDF si envoyé. Retourne (ok, cheminVirtuel)
        private Tuple<bool, string> TrySavePdf(HttpPostedFileBase fichier)
        {
            if (fichier == null || fichier.ContentLength == 0) return Tuple.Create(false, "");
            if (fichier.ContentLength > MaxPdfBytes) { ModelState.AddModelError("", "Le fichier dépasse 10MB."); return Tuple.Create(false, ""); }
            var ext = Path.GetExtension(fichier.FileName)?.ToLowerInvariant();
            if (ext != ".pdf") { ModelState.AddModelError("", "Seuls les fichiers PDF sont autorisés."); return Tuple.Create(false, ""); }

            var root = Server.MapPath(UploadFolderVirtual); if (!Directory.Exists(root)) Directory.CreateDirectory(root);
            var safe = Path.GetFileNameWithoutExtension(fichier.FileName);
            var name = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safe}.pdf";
            var phys = Path.Combine(root, name); fichier.SaveAs(phys);
            return Tuple.Create(true, VirtualPathUtility.ToAbsolute($"{UploadFolderVirtual}/{name}"));
        }
    }
}
