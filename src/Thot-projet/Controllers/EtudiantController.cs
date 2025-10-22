using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Infrastructure;
using Thot_projet.Models;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Etudiant")]
    public class EtudiantController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Dashboard()
        {
            int uid = (int)(Session["UserId"] ?? 0);

            var vm = new EtudiantDashboardVM
            {
                // Mis cursos (inscripciones)
                MesInscriptions = db.Inscriptions
                    .Where(i => i.UtilisateurId == uid)
                    .OrderByDescending(i => i.InscritLe)
                    .Take(10)
                    .ToList(),

                // Mis preguntas
                MesQuestions = db.Questions
                    .Where(q => q.EtudiantId == uid)
                    .OrderByDescending(q => q.Creele)
                    .Take(10)
                    .ToList()
            };

            return View(vm);
        }
    }
}
