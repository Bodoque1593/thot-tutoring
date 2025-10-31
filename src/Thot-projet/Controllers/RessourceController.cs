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
        private const int MaxPdfBytes = 10 * 1024 * 1024; // 10MB
        private const string UploadFolderVirtual = "~/Content/uploads/resources";

        public ActionResult Index()
        {
            var data = db.Ressources
                         .Include(r => r.ModuleCours.Cours)
                         .OrderBy(r => r.ModuleCours.Cours.Nom)
                         .ThenBy(r => r.ModuleCours.Numero)
                         .ThenBy(r => r.Titre)
                         .ToList();
            return View(data);
        }

        public ActionResult Details(int id)
        {
            var r = db.Ressources
                      .Include(x => x.ModuleCours.Cours)
                      .FirstOrDefault(x => x.id == id);
            if (r == null) return HttpNotFound();
            return View(r);
        }

        // ---------- CREATE ----------
        [HttpGet]
        public ActionResult Create(int? moduleId = null)
        {
            ViewBag.ModuleCoursId = ModuleSelectList(moduleId);
            ViewBag.UiHelp = true;
            return View(new Ressource());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ModuleCoursId,Type,Titre,url")] Ressource r, HttpPostedFileBase fichier)
        {
            // Si suben PDF, lo guardamos y ponemos la ruta en r.url
            var uploadedPath = TrySavePdf(fichier);
            if (uploadedPath.Item1)
            {
                r.url = uploadedPath.Item2;            // /Content/uploads/resources/xxxxx.pdf
                if (string.IsNullOrWhiteSpace(r.Type)) // por si no llenan el campo
                    r.Type = "PDF";
            }

            // Reglas: Debe venir URL o PDF. Si ambos vacíos -> error
            if (string.IsNullOrWhiteSpace(r.url))
                ModelState.AddModelError("url", "Indiquez un lien OU téléversez un PDF.");

            if (!ModelState.IsValid)
            {
                ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId);
                ViewBag.UiHelp = true;
                ViewBag.UploadError = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(r);
            }

            try
            {
                db.Ressources.Add(r);
                db.SaveChanges();
                TempData["ok"] = "Ressource créée.";
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Mostrar claramente qué falló en EF
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        ModelState.AddModelError(ve.PropertyName ?? "", ve.ErrorMessage);

                ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId);
                ViewBag.UiHelp = true;
                return View(r);
            }
        }



        // ---------- EDIT ----------
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var r = db.Ressources
                      .Include(x => x.ModuleCours.Cours)
                      .FirstOrDefault(x => x.id == id);
            if (r == null) return HttpNotFound();

            ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId);
            return View(r);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,ModuleCoursId,Type,Titre,url")] Ressource r, HttpPostedFileBase fichier)
        {
            // Si suben un nuevo PDF, se sustituye la url por la ruta del nuevo
            var uploadedPath = TrySavePdf(fichier);
            if (uploadedPath.Item1)
            {
                r.url = uploadedPath.Item2;
                if (string.IsNullOrWhiteSpace(r.Type))
                    r.Type = "PDF";
            }

            // Debe existir al menos una cosa: URL o PDF (ya transformado en url)
            if (string.IsNullOrWhiteSpace(r.url))
                ModelState.AddModelError("url", "Indiquez un lien OU téléversez un PDF.");

            if (!ModelState.IsValid)
            {
                ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId);
                return View(r);
            }

            try
            {
                db.Entry(r).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["ok"] = "Ressource modifiée.";
                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        ModelState.AddModelError(ve.PropertyName ?? "", ve.ErrorMessage);

                ViewBag.ModuleCoursId = ModuleSelectList(r.ModuleCoursId);
                return View(r);
            }
        }


        // ---------- DELETE ----------
        public ActionResult Delete(int id)
        {
            var r = db.Ressources
                      .Include(x => x.ModuleCours.Cours)
                      .FirstOrDefault(x => x.id == id);
            if (r == null) return HttpNotFound();
            return View(r);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var r = db.Ressources.Find(id);
            if (r != null)
            {
                db.Ressources.Remove(r);
                db.SaveChanges();
                TempData["ok"] = "Ressource supprimée.";
            }
            return RedirectToAction("Index");
        }

        // ---------- helpers ----------
        private SelectList ModuleSelectList(int? selectedId = null)
        {
            var items = db.ModulesCours
                          .Include(m => m.Cours)
                          .OrderBy(m => m.Cours.Nom)
                          .ThenBy(m => m.Numero)
                          .Select(m => new
                          {
                              m.id,
                              Texte = (m.Cours.Nom + " – Module " + m.Numero + " : " + m.Titre)
                          })
                          .ToList();
            return new SelectList(items, "id", "Texte", selectedId);
        }

        /// <summary>
        /// Intenta guardar el PDF. Devuelve (ok, virtualPath)
        /// </summary>
        private Tuple<bool, string> TrySavePdf(HttpPostedFileBase fichier)
        {
            if (fichier == null || fichier.ContentLength == 0) return Tuple.Create(false, "");

            // tamaño
            if (fichier.ContentLength > MaxPdfBytes)
            {
                ModelState.AddModelError("", "Le fichier dépasse la taille maximale (10MB).");
                return Tuple.Create(false, "");
            }

            // extensión/MIME
            var ext = Path.GetExtension(fichier.FileName)?.ToLowerInvariant();
            if (ext != ".pdf")
            {
                ModelState.AddModelError("", "Seuls les fichiers PDF sont autorisés.");
                return Tuple.Create(false, "");
            }

            // carpeta
            var root = Server.MapPath(UploadFolderVirtual);
            if (!Directory.Exists(root)) Directory.CreateDirectory(root);

            // nombre final
            var safeName = Path.GetFileNameWithoutExtension(fichier.FileName);
            var finalName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeName}.pdf";
            var physical = Path.Combine(root, finalName);
            fichier.SaveAs(physical);

            var virtualPath = VirtualPathUtility.ToAbsolute($"{UploadFolderVirtual}/{finalName}");
            return Tuple.Create(true, virtualPath);
        }
    }
}
