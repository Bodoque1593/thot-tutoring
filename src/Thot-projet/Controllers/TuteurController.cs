using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Tuteur")]
    public class TuteurController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Tableau de bord tuteur : questions ouvertes + sessions de clavardage
        public ActionResult Dashboard()
        {
            int uid = (int)(Session["UserId"] ?? 0);

            var vm = new TuteurDashboardVM
            {
                QuestionsOuvertes = db.Questions.Where(q => !q.EstResolvee)
                                                .OrderByDescending(q => q.Creele).Take(10).ToList(),
                MesChats = db.SessionsClavardage.Where(s => s.TuteurId == uid)
                                                .OrderByDescending(s => s.DemarreLe).Take(10).ToList()
            };
            return View(vm);
        }
    }
}
