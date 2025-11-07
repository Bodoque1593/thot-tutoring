using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Etudiant")]
    public class EtudiantController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Tableau de bord étudiant : dernières inscriptions + questions
        public ActionResult Dashboard()
        {
            int uid = (int)(Session["UserId"] ?? 0);

            var vm = new Thot_projet.Models.EtudiantDashboardVM
            {
                MesInscriptions = db.Inscriptions
                                    .Include(i => i.Cours)
                                    .Where(i => i.UtilisateurId == uid)
                                    .OrderByDescending(i => i.InscritLe)
                                    .Take(10).ToList(),

                MesQuestions = db.Questions
                                 .Where(q => q.EtudiantId == uid)
                                 .OrderByDescending(q => q.Creele)
                                 .Take(10).ToList()
            };
            return View(vm);
        }
    }
}
