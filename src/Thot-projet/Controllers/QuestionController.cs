using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Thot_projet.Data;
using Thot_projet.Models;
// ... demás using que ya tengas

public class QuestionController : Controller
{
    private readonly AppDbContext db = new AppDbContext();




    // >>> EN TU QuestionController.cs  (pega dentro de la clase) <<<

    [HttpGet]
    public ActionResult Index()
    {
        // Trae todas las preguntas con navegación para mostrar datos
        // Ajusta el nombre de tu DbContext si no es 'db'
        var questions = db.Questions
            .ToList(); // no forzamos Include para no romper tu proyecto

        return View(questions);
    }

    // GET: /Question/Details/5
    [HttpGet]
    public ActionResult Details(int id)
    {
        // IMPORTANTE: Find usa la PK real (no necesita que se llame "Id")
        var q = db.Questions.Find(id);
        if (q == null) return HttpNotFound();
        return View(q);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Answer(int id, string contenu)
    {
        if (string.IsNullOrWhiteSpace(contenu))
        {
            TempData["err"] = "La réponse est vide.";
            return RedirectToAction("Details", new { id });
        }

        var q = db.Questions.Find(id);
        if (q == null) return HttpNotFound();

        var rep = new Reponse
        {
            // si tu modelo usa FK:
            QuestionId = id,      // <-- deja esto si tu Reponse tiene QuestionId
            Contenu = contenu
            // si NO tiene QuestionId pero sí navegación:
            // Question = q
        };

        db.Reponses.Add(rep);
        db.SaveChanges();

        TempData["ok"] = "Réponse envoyée.";
        return RedirectToAction("Details", new { id });
    }


    // ... tus otras acciones
}
