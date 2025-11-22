using System;
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
    public class PaiementController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // HttpClient pour parler au microservice Python (Stripe)
        private static readonly HttpClient http = new HttpClient
        {
            // Assure-toi que c’est le même port que tu utilises avec uvicorn
            BaseAddress = new Uri("http://127.0.0.1:8004/")
        };

        // --------------------------------------------------------------------
        //  PAIEMENTS CLASSIQUES (déjà existants)
        // --------------------------------------------------------------------

        // Paiements de l'utilisateur courant
        public ActionResult Index()
        {
            int uid = (int)(Session["UserId"] ?? 0);
            var list = db.Paiements
                         .Where(p => p.UtilisateurId == uid)
                         .OrderByDescending(p => p.PayeLe)
                         .ToList();
            return View(list);
        }

        // Création guidée depuis l’inscription (mode manuel, sans Stripe)
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

        [HttpPost, ValidateAntiForgeryToken, RoleAuthorize("Etudiant")]
        public ActionResult Create(decimal Montant, string Monnaie, string Statut)
        {
            int uid = (int)(Session["UserId"] ?? 0);

            if (Montant <= 0)
                ModelState.AddModelError("Montant", "Le montant doit être > 0.");

            if (string.IsNullOrWhiteSpace(Monnaie))
                ModelState.AddModelError("Monnaie", "La monnaie est requise.");

            if (string.IsNullOrWhiteSpace(Statut))
                ModelState.AddModelError("Statut", "Le statut est requis.");

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

        // Vue “IndexAll” (tuteur)
        [RoleAuthorize("Tuteur")]
        public ActionResult IndexAll()
            => View(db.Paiements.OrderByDescending(p => p.PayeLe).ToList());

        // --------------------------------------------------------------------
        //  NOUVEAU : PAIEMENT VIA MICRO-SERVICE STRIPE
        // --------------------------------------------------------------------

        // GET /Paiement/Payer/5  (5 = id du cours)
        [RoleAuthorize("Etudiant")]
        public async Task<ActionResult> Payer(int id)
        {
            // Vérifier utilisateur connecté
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Auth");

            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Utilisateurs.Find(userId);
            if (user == null)
            {
                TempData["err"] = "Utilisateur introuvable.";
                return RedirectToAction("Browse", "Inscription");
            }

            // Préparer le JSON à envoyer au microservice Python
            var payload = new
            {
                CoursId = id,
                UtilisateurEmail = user.Email,
                Currency = "cad"
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await http.PostAsync("create-checkout-session", content);
                if (!response.IsSuccessStatusCode)
                {
                    TempData["err"] = "Erreur lors de la création du paiement Stripe.";
                    return RedirectToAction("Browse", "Inscription");
                }

                var body = await response.Content.ReadAsStringAsync();
                dynamic dto = JsonConvert.DeserializeObject(body);
                string checkoutUrl = dto.checkout_url;

                // Redirection directe vers Stripe
                return Redirect(checkoutUrl);
            }
            catch (Exception ex)
            {
                TempData["err"] = "Erreur paiement : " + ex.Message;
                return RedirectToAction("Browse", "Inscription");
            }
        }

        // success_url du microservice -> /Paiement/Success?session_id=xxx
        [RoleAuthorize("Etudiant")]
        public ActionResult Success(string session_id)
        {
            TempData["ok"] = "Paiement complété (session : " + session_id + ").";
            // Plus tard: enregistrer automatiquement un Paiement si tu veux.
            return RedirectToAction("Index", "Inscription");
        }

        // cancel_url du microservice -> /Paiement/Cancel
        [RoleAuthorize("Etudiant")]
        public ActionResult Cancel()
        {
            TempData["err"] = "Paiement annulé.";
            return RedirectToAction("Browse", "Inscription");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
