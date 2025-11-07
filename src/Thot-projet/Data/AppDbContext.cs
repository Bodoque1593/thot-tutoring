using System.Data.Entity;
using Thot_projet.Models;

namespace Thot_projet.Data
{
    /// <summary>
    /// Contexte EF de base de l’application.
    /// Chaîne: "DefaultConnection" (Web.config).
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("DefaultConnection") { }

        // --- Tables (DbSet) -------------------------------------------------
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Cours> Cours { get; set; }
        public DbSet<ModuleCours> ModulesCours { get; set; }
        public DbSet<Ressource> Ressources { get; set; }
        public DbSet<Inscription> Inscriptions { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Reponse> Reponses { get; set; }
        public DbSet<Abonnement> Abonnements { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<SessionClavardage> SessionsClavardage { get; set; }
        public DbSet<ProfilTuteur> ProfilsTuteur { get; set; }
        public DbSet<EntreeFAQ> EntreesFAQ { get; set; }

        // --- Configuration des relations -----------------------------------
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1) Cours (1) -> (N) ModuleCours  (pas de cascade à la suppression)
            modelBuilder.Entity<ModuleCours>()
                .HasRequired(m => m.Cours)               // un module appartient à un cours
                .WithMany(c => c.Modules)                // un cours possède plusieurs modules
                .HasForeignKey(m => m.CoursId)           // FK = ModuleCours.CoursId
                .WillCascadeOnDelete(false);             // suppression du cours ≠ suppression des modules

            // 2) ModuleCours (1) -> (N) Ressource
            modelBuilder.Entity<Ressource>()
                .HasRequired(r => r.ModuleCours)
                .WithMany(m => m.Ressources)
                .HasForeignKey(r => r.ModuleCoursId)
                .WillCascadeOnDelete(false);

            // 3) Cours (1) -> (N) Question
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Cours)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.CoursId)
                .WillCascadeOnDelete(false);

            // 4) Ressource (1) -> (N) Question
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Ressource)
                .WithMany(r => r.Questions)
                .HasForeignKey(q => q.RessourceId)
                .WillCascadeOnDelete(false);

            // 5) Utilisateur (Etudiant) (1) -> (N) Question
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Etudiant)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.EtudiantId)
                .WillCascadeOnDelete(false);

            // 6) Question (1) -> (N) Reponse  (ici cascade activée)
            modelBuilder.Entity<Reponse>()
                .HasRequired(r => r.Question)
                .WithMany(q => q.Reponses)
                .HasForeignKey(r => r.QuestionId)
                .WillCascadeOnDelete(true);

            // 7) Utilisateur (Tuteur) (1) -> (N) Reponse
            modelBuilder.Entity<Reponse>()
                .HasRequired(r => r.Tuteur)
                .WithMany(u => u.Reponses)
                .HasForeignKey(r => r.TuteurId)
                .WillCascadeOnDelete(false);

            // 8) Utilisateur (1) -> (N) Inscription
            modelBuilder.Entity<Inscription>()
                .HasRequired(i => i.Utilisateur)
                .WithMany(u => u.Inscriptions)
                .HasForeignKey(i => i.UtilisateurId)
                .WillCascadeOnDelete(false);

            // 9) Cours (1) -> (N) Inscription
            modelBuilder.Entity<Inscription>()
                .HasRequired(i => i.Cours)
                .WithMany(c => c.Inscriptions)
                .HasForeignKey(i => i.CoursId)
                .WillCascadeOnDelete(false);

            // 10) Utilisateur (1) -> (N) Paiement
            modelBuilder.Entity<Paiement>()
                .HasRequired(p => p.Utilisateur)
                .WithMany(u => u.Paiements)
                .HasForeignKey(p => p.UtilisateurId)
                .WillCascadeOnDelete(false);

            // 11) SessionClavardage.Etudiant (N:1)
            modelBuilder.Entity<SessionClavardage>()
                .HasRequired(s => s.Etudiant)
                .WithMany()
                .HasForeignKey(s => s.EtudiantId)
                .WillCascadeOnDelete(false);

            // 12) SessionClavardage.Tuteur (N:1)
            modelBuilder.Entity<SessionClavardage>()
                .HasRequired(s => s.Tuteur)
                .WithMany()
                .HasForeignKey(s => s.TuteurId)
                .WillCascadeOnDelete(false);
        }
    }
}
