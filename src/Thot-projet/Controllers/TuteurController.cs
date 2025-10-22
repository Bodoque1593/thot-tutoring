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

        public ActionResult Dashboard()
        {
            int uid = (int)(Session["UserId"] ?? 0);

            var vm = new TuteurDashboardVM
            {
                // Cola de preguntas no resueltas (se pueden filtrar por materias del tutor si más adelante modelas esa relación)
                QuestionsOuvertes = db.Questions
                    .Where(q => !q.EstResolvee)
                    .OrderByDescending(q => q.Creele)
                    .Take(10)
                    .ToList(),

                // Mis sesiones como tutor
                MesChats = db.SessionsClavardage
                    .Where(s => s.TuteurId == uid)
                    .OrderByDescending(s => s.DemarreLe)
                    .Take(10)
                    .ToList()
            };

            return View(vm);
        }
    }
}
