using System;
using System.Collections.Generic;
using Thot_projet.Models;

namespace Thot_projet.Models
{
    public class TuteurDashboardVM
    {
        public IList<Question> QuestionsOuvertes { get; set; } = new List<Question>();
        public IList<SessionClavardage> MesChats { get; set; } = new List<SessionClavardage>();
    }

    public class EtudiantDashboardVM
    {
        public IList<Inscription> MesInscriptions { get; set; } = new List<Inscription>();
        public IList<Question> MesQuestions { get; set; } = new List<Question>();
    }
}
