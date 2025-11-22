using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Thot_projet.Infrastructure;

namespace Thot_projet.Controllers
{
    [RoleAuthorize("Tuteur")]
    public class StatsController : Controller
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8003")
        };

        // GET: /Stats
        public async Task<ActionResult> Index()
        {
            try
            {
                var response = await httpClient.GetAsync("/stats/overview");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Erreur lors de l'appel au microservice de statistiques.";
                    return View();
                }

                var json = await response.Content.ReadAsStringAsync();
                // Usamos JObject (sin nuevos ViewModels)
                var obj = JObject.Parse(json);

                ViewBag.TotalCours = (int)obj["totalCours"];
                ViewBag.TotalUtilisateurs = (int)obj["totalUtilisateurs"];
                ViewBag.TotalQuestions = (int)obj["totalQuestions"];
                ViewBag.QuestionsOuvertes = (int)obj["questionsOuvertes"];
                ViewBag.TotalFaq = (int)obj["totalFaq"];

                // topCours: lo convertimos a lista de pares (Nom, NbInscrits)
                var list = new List<KeyValuePair<string, int>>();
                foreach (var c in obj["topCours"])
                {
                    string nom = (string)c["nom"];
                    int nb = (int)c["nbInscriptions"];
                    list.Add(new KeyValuePair<string, int>(nom, nb));
                }
                ViewBag.TopCours = list;

                return View();
            }
            catch (Exception)
            {
                ViewBag.Error = "Erreur de communication avec le microservice de statistiques.";
                return View();
            }
        }
    }
}
