using System;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    // -> ahora permite Etudiant y Tuteur
    [RoleAuthorize("Etudiant", "Tuteur")]
    public class PaiementController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // LISTA DE PAGOS (para el usuario actual)
        public ActionResult Index()
        {
            int uid = (int)(Session["UserId"] ?? 0);
            var list = db.Paiements
                         .Where(p => p.UtilisateurId == uid)
                         .OrderByDescending(p => p.PayeLe)
                         .ToList();

            return View(list);  // Views/Paiement/Index.cshtml (ya la tienes)
        }

        // GET: /Paiement/Create  (pre-carga desde inscripción)
        [RoleAuthorize("Etudiant")]
        public ActionResult Create(decimal? Montant, string Monnaie = "CAD", string Statut = "Payé")
        {
            ViewBag.Monnaies = new[] { "CAD", "USD", "EUR" };
            ViewBag.Statuts = new[] { "Payé", "En attente", "Annulé" };

            ViewBag.Montant = Montant ?? 0.01m;
            ViewBag.Monnaie = Monnaie;
            ViewBag.Statut = Statut;
            return View();
        }

        // POST: /Paiement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Etudiant")]
        public ActionResult Create(decimal Montant, string Monnaie, string Statut)
        {
            int uid = (int)(Session["UserId"] ?? 0);

            if (Montant <= 0) ModelState.AddModelError("Montant", "Le montant doit être > 0.");
            if (string.IsNullOrWhiteSpace(Monnaie)) ModelState.AddModelError("Monnaie", "La monnaie est requise.");
            if (string.IsNullOrWhiteSpace(Statut)) ModelState.AddModelError("Statut", "Le statut est requis.");

            if (!ModelState.IsValid)
            {
                ViewBag.Monnaies = new[] { "CAD", "USD", "EUR" };
                ViewBag.Statuts = new[] { "Payé", "En attente", "Annulé" };
                return View();
            }

            db.Paiements.Add(new Paiement
            {
                UtilisateurId = uid,
                Montant = Montant,
                Monnaie = Monnaie.Trim(),
                Statut = Statut.Trim(),
                PayeLe = DateTime.UtcNow
            });
            db.SaveChanges();

            TempData["ok"] = "Paiement enregistré.";
            return RedirectToAction("Index");
        }
    }
}
