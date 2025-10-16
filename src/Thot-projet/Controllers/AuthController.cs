using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;                
using Thot_projet.Data;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: /Auth/Login
       
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login
       
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var email = (model.Email ?? "").Trim().ToLower();
            var pass = (model.Motdepasse ?? "").Trim();

            var user = db.Utilisateurs
                         .FirstOrDefault(u => u.Email.Trim().ToLower() == email);

            if (user == null || string.IsNullOrWhiteSpace(user.Motdepasse) ||
                !string.Equals(user.Motdepasse.Trim(), pass, System.StringComparison.Ordinal))
            {
                ModelState.AddModelError("", "Identifiants invalides.");
                return View(model);
            }

            System.Web.Security.FormsAuthentication.SetAuthCookie(user.Email, model.Mesouvenir);

            Session["UserId"] = user.id;
            Session["UserRole"] = user.Role; // "Tuteur" | "Etudiant"
            Session["UserName"] = string.IsNullOrWhiteSpace(user.Nomcomplet) ? user.Email : user.Nomcomplet;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Cours");
        }

        // GET: /Auth/Logout
        
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
