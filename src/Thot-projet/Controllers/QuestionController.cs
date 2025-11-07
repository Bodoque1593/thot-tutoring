using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    // Étudiant et Tuteur peuvent entrer ici; cada action afina permisos
    [RoleAuthorize("Etudiant", "Tuteur")]
    public class QuestionController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // --- utilitaires simples ---
        private int CurrentUserId() => (int)(Session["UserId"] ?? 0);
        private string CurrentRole() => Convert.ToString(Session["UserRole"] ?? "");

        // ================== ROUTAGE POR ROL ==================
        // Redirige al listado correcto según el rol actual
        [HttpGet]
        public ActionResult Index()
        {
            var role = CurrentRole();
            if (string.Equals(role, "Tuteur", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction(nameof(Index_Tuteur));
            // por defecto, estudiante
            return RedirectToAction(nameof(Index_Etudiant));
        }

        // === Liste pour l'étudiant (MES questions) ===
        [Authorize]
        public ActionResult Index_Etudiant()
        {
            int uid = (int)(Session["UserId"] ?? 0);

            var data = db.Questions
                .Where(q => q.EtudiantId == uid)
                .OrderByDescending(q => q.Creele)
                .Include(q => q.Cours)        // ← para mostrar nombre del curso
                .Include(q => q.Ressource)    // ← para mostrar título de la ressource
                .ToList();

            return View(data); // Views/Question/Index_Etudiant.cshtml
        }

        // LISTA: Tuteur (no resueltas) → /Question/Index_Tuteur
        [HttpGet]
        [Authorize]
        public ActionResult Index_Tuteur()
        {
            var data = db.Questions
                .Where(q => !q.EstResolvee)
                .OrderByDescending(q => q.Creele)
                .Include(q => q.Etudiant)     // ← para ver quién preguntó
                .Include(q => q.Cours)
                .Include(q => q.Ressource)
                .ToList();

            return View(data); // Views/Question/Index_Tuteur.cshtml
        }

        // ================== DÉTAILS & RÉPONSE ==================
        // DETALLE (con respuestas) → /Question/Details/{id}
        [HttpGet]
        [Authorize]
        public ActionResult Details(int id)
        {
            var q = db.Questions
                .Include(x => x.Cours)
                .Include(x => x.Ressource)
                .Include(x => x.Reponses)    // ← para que salgan en la vista
                .FirstOrDefault(x => x.id == id);

            if (q == null) return HttpNotFound();
            return View(q); // tu vista Details actual
        }

        // Tuteur responde; si tu modelo Reponse tiene fecha no-null, la fijamos a "ahora" para evitar DateTime.Min
        // RESPONDER (tuteur)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Answer(int id, string contenu, bool resoudre = true)
        {
            if (string.IsNullOrWhiteSpace(contenu))
            {
                TempData["err"] = "La réponse est vide.";
                return RedirectToAction("Details", new { id });
            }

            var q = db.Questions.Find(id);
            if (q == null) return HttpNotFound();

            int tuteurId = (int)(Session["UserId"] ?? 0);

            db.Reponses.Add(new Reponse
            {
                QuestionId = id,
                TuteurId = tuteurId,
                Contenu = contenu.Trim(),
                Creele = DateTime.UtcNow
            });

            // ↓↓↓ Simple y sin tocar más vistas: por defecto la marcamos resuelta
            if (resoudre) q.EstResolvee = true;

            db.SaveChanges();
            TempData["ok"] = "Réponse envoyée.";
            return RedirectToAction("Details", new { id });
        }
        // ================== CRÉER (ÉTUDIANT) ==================
        // GET: /Question/Create  (tu vue Create ya la tienes)
        [HttpGet, RoleAuthorize("Etudiant")]
        public ActionResult Create()
        {
            ViewBag.Cours = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom");
            ViewBag.Ressources = new SelectList(db.Ressources.OrderBy(r => r.Titre), "id", "Titre");
            return View(new Question());
        }

        // POST: /Question/Create
        [HttpPost, ValidateAntiForgeryToken, RoleAuthorize("Etudiant")]
        public ActionResult Create(int CoursId, int RessourceId, string Contenu)
        {
            if (CoursId <= 0) ModelState.AddModelError("CoursId", "Le cours est requis.");
            if (RessourceId <= 0) ModelState.AddModelError("RessourceId", "La ressource est requise.");
            if (string.IsNullOrWhiteSpace(Contenu)) ModelState.AddModelError("Contenu", "Le contenu est requis.");

            if (!ModelState.IsValid)
            {
                ViewBag.Cours = new SelectList(db.Cours.OrderBy(c => c.Nom), "id", "Nom", CoursId);
                ViewBag.Ressources = new SelectList(db.Ressources.OrderBy(r => r.Titre), "id", "Titre", RessourceId);
                return View(new Question { CoursId = CoursId, RessourceId = RessourceId, Contenu = Contenu });
            }

            var q = new Question
            {
                CoursId = CoursId,
                RessourceId = RessourceId,
                EtudiantId = CurrentUserId(),
                Contenu = Contenu.Trim(),
                Creele = DateTime.UtcNow,
                EstResolvee = false
            };

            db.Questions.Add(q);
            db.SaveChanges();

            TempData["ok"] = "Question créée.";
            return RedirectToAction(nameof(Details), new { id = q.id });
        }

        // ================== DEMANDE DE RENCONTRE ==================
        // >>> Mantengo tu firma EXACTA (sin 'id' obligatorio). <<<
        // GET: /Question/DemandeRencontre
        [HttpGet, Authorize]
        public ActionResult DemandeRencontre(int? coursId = null)
        {
            // FR: je remplis les listes comme avant, pour ta vue
            ViewBag.Cours = new SelectList(
                db.Cours.OrderBy(c => c.Nom).ToList(), "id", "Nom", coursId);

            ViewBag.Ressources = new SelectList(
                db.Ressources.OrderBy(r => r.Titre).ToList(), "id", "Titre");

            return View(); // Views/Question/DemandeRencontre.cshtml (ya existe)
        }

        // POST: /Question/DemandeRencontre
        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public ActionResult DemandeRencontre(int CoursId, int RessourceId, DateTime? DateHeure, string Lieu, string Note)
        {
            if (CoursId <= 0) ModelState.AddModelError("CoursId", "Choisissez un cours.");
            if (RessourceId <= 0) ModelState.AddModelError("RessourceId", "Choisissez une ressource.");
            if (string.IsNullOrWhiteSpace(Lieu)) ModelState.AddModelError("Lieu", "Indiquez un lieu.");

            if (!ModelState.IsValid)
            {
                ViewBag.Cours = new SelectList(db.Cours.OrderBy(c => c.Nom).ToList(), "id", "Nom", CoursId);
                ViewBag.Ressources = new SelectList(db.Ressources.OrderBy(r => r.Titre).ToList(), "id", "Titre", RessourceId);
                return View();
            }

            var uid = CurrentUserId();
            var whenTxt = DateHeure.HasValue ? DateHeure.Value.ToString("yyyy-MM-dd HH:mm") : "(à convenir)";
            var noteTxt = string.IsNullOrWhiteSpace(Note) ? "" : (" | Note: " + Note.Trim());

            var contenu = $"[RENCONTRE] Demande de rencontre — Quand: {whenTxt} | Lieu: {Lieu?.Trim()}{noteTxt}";

            var q = new Question
            {
                CoursId = CoursId,
                RessourceId = RessourceId,
                EtudiantId = uid,
                Contenu = contenu,
                Creele = DateTime.UtcNow,
                EstResolvee = false
            };

            db.Questions.Add(q);
            db.SaveChanges();

            TempData["ok"] = "Demande de rencontre envoyée.";
            return RedirectToAction(nameof(Details), new { id = q.id });
        }

        // =========================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
