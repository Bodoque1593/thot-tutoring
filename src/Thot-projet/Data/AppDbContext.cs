using System.Data.Entity;
using Thot_projet.Models;

namespace Thot_projet.Data
{
    // FR: Contexte principal d'EF6. Il gère la connexion à la BD et le suivi des entités.
    public class AppDbContext : DbContext
    {
        // FR: Utiliser la chaîne "DefaultConnection" définie dans Web.config
        public AppDbContext() : base("DefaultConnection") { }

        // FR: DbSet = table logique (EF crée les tables à partir des classes, approche Code-First)
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Cours> Cours { get; set; }
        public DbSet<ModuleCours> ModulesCours { get; set; }
        public DbSet<Ressource> Ressources { get; set; } // ancien nom: RessourcesDefault
        public DbSet<Inscription> Inscriptions { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Reponse> Reponses { get; set; }
        public DbSet<Abonnement> Abonnements { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<SessionClavardage> SessionsClavardage { get; set; }
        public DbSet<ProfilTuteur> ProfilsTuteur { get; set; }
        public DbSet<EntreeFAQ> EntreesFAQ { get; set; }

        // FR: Configuration fluide des relations pour éviter les cascades multiples côté SQL Server.
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FR: 1:N Cours -> ModuleCours (pas de cascade à la suppression)
            modelBuilder.Entity<ModuleCours>()
                .HasRequired(m => m.Cours)            // FR: un module appartient à un cours (FK requise)
                .WithMany(c => c.Modules)             // FR: un cours possède plusieurs modules
                .HasForeignKey(m => m.CoursId)        // FR: clé étrangère = ModuleCours.CoursId
                .WillCascadeOnDelete(false);          // FR: la suppression du cours ne supprime pas les modules

            // FR: 1:N ModuleCours -> Ressource (pas de cascade)
            modelBuilder.Entity<Ressource>()
                .HasRequired(r => r.ModuleCours)
                .WithMany(m => m.Ressources)
                .HasForeignKey(r => r.ModuleCoursId)
                .WillCascadeOnDelete(false);

            // FR: 1:N Cours -> Question (pas de cascade)
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Cours)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.CoursId)
                .WillCascadeOnDelete(false);

            // FR: 1:N Ressource -> Question (pas de cascade)
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Ressource)
                .WithMany(r => r.Questions)
                .HasForeignKey(q => q.RessourceId)
                .WillCascadeOnDelete(false);

            // FR: 1:N Utilisateur(Etudiant) -> Question (pas de cascade)
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Etudiant)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.EtudiantId)
                .WillCascadeOnDelete(false);

            // FR: 1:N Question -> Reponse (cascade OK ici)
            modelBuilder.Entity<Reponse>()
                .HasRequired(r => r.Question)
                .WithMany(q => q.Reponses)
                .HasForeignKey(r => r.QuestionId)
                .WillCascadeOnDelete(true);

            // FR: 1:N Utilisateur(Tuteur) -> Reponse (pas de cascade)
            modelBuilder.Entity<Reponse>()
                .HasRequired(r => r.Tuteur)
                .WithMany(u => u.Reponses)
                .HasForeignKey(r => r.TuteurId)
                .WillCascadeOnDelete(false);

            // FR: N:N via Inscription (pas de cascade des deux côtés)
            modelBuilder.Entity<Inscription>()
                .HasRequired(i => i.Utilisateur)
                .WithMany(u => u.Inscriptions)
                .HasForeignKey(i => i.UtilisateurId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Inscription>()
                .HasRequired(i => i.Cours)
                .WithMany(c => c.Inscriptions)
                .HasForeignKey(i => i.CoursId)
                .WillCascadeOnDelete(false);

            // FR: 1:N Utilisateur -> Paiement (pas de cascade)
            modelBuilder.Entity<Paiement>()
                .HasRequired(p => p.Utilisateur)
                .WithMany(u => u.Paiements)
                .HasForeignKey(p => p.UtilisateurId)
                .WillCascadeOnDelete(false);

            // FR: 1:N Utilisateur(Etudiant) -> SessionClavardage (pas de cascade)
            modelBuilder.Entity<SessionClavardage>()
                .HasRequired(s => s.Etudiant)
                .WithMany()
                .HasForeignKey(s => s.EtudiantId)
                .WillCascadeOnDelete(false);

            // FR: 1:N Utilisateur(Tuteur) -> SessionClavardage (pas de cascade)
            modelBuilder.Entity<SessionClavardage>()
                .HasRequired(s => s.Tuteur)
                .WithMany()
                .HasForeignKey(s => s.TuteurId)
                .WillCascadeOnDelete(false);
        }
    }
}
