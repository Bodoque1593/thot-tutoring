using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Etudiant", "Tuteur")]
    public class PaiementController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        private static readonly string[] MONNAIES = new[] { "CAD", "USD", "EUR" };
        private static readonly string[] STATUTS = new[] { "Payé", "En attente", "Annulé" };

        [RoleAuthorize("Etudiant")]
        public ActionResult Create(decimal? Montant, string Monnaie = "CAD", string Statut = "Payé")
        {
            ViewBag.Monnaies = MONNAIES;
            ViewBag.Statuts = STATUTS;

            // mostramos con coma para fr-CA (lo que ves en la UI)
            var fr = CultureInfo.GetCultureInfo("fr-CA");
            ViewBag.MontantStr = (Montant ?? 0.01m).ToString("0.##", fr);
            ViewBag.Monnaie = MONNAIE_OK(Monnaie) ? Monnaie : "CAD";
            ViewBag.Statut = STATUT_OK(Statut) ? Statut : "Payé";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Etudiant")]
        public ActionResult Create(string Montant, string Monnaie, string Statut)
        {
            int uid = (int)(Session["UserId"] ?? 0);

            // Parsear monto aceptando COMA o PUNTO
            decimal montantVal = 0m;
            bool parsed =
                decimal.TryParse(Montant, NumberStyles.Number, CultureInfo.GetCultureInfo("fr-CA"), out montantVal) ||
                decimal.TryParse(Montant, NumberStyles.Number, CultureInfo.GetCultureInfo("en-US"), out montantVal);

            if (!parsed || montantVal <= 0)
                ModelState.AddModelError("Montant", "Montant invalide (> 0).");

            if (!MONNAIE_OK(Monnaie))
                ModelState.AddModelError("Monnaie", "Monnaie invalide.");

            if (!STATUT_OK(Statut))
                ModelState.AddModelError("Statut", "Statut invalide.");

            if (!ModelState.IsValid)
            {
                ViewBag.Monnaies = MONNAIES;
                ViewBag.Statuts = STATUTS;
                ViewBag.MontantStr = Montant;  // conservar lo que escribió
                ViewBag.Monnaie = Monnaie;
                ViewBag.Statut = Statut;
                return View();
            }

            db.Paiements.Add(new Paiement
            {
                UtilisateurId = uid,
                Montant = montantVal,
                Monnaie = Monnaie.Trim(),
                Statut = Statut.Trim(),
                PayeLe = DateTime.UtcNow
            });
            db.SaveChanges();

            TempData["ok"] = "Paiement enregistré.";
            return RedirectToAction("Index");
        }

        private bool MONNAIE_OK(string m) => MONNAIES.Contains((m ?? "").Trim());
        private bool STATUT_OK(string s) => STATUTS.Contains((s ?? "").Trim());

        // Listado simple de pagos del usuario
        [RoleAuthorize("Etudiant")]
        public ActionResult Index()
        {
            int uid = (int)(Session["UserId"] ?? 0);
            var list = db.Paiements.Where(p => p.UtilisateurId == uid)
                                   .OrderByDescending(p => p.PayeLe)
                                   .ToList();
            return View(list);
        }
    }
}
