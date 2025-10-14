using System.Data.Entity;
using Thot_projet.Models;

namespace Thot_projet.Data
{
  
    public class AppDbContext : DbContext
    {
       
        public AppDbContext() : base("DefaultConnection") { }

        
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

    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1:N Cours -> ModuleCours (pas de cascade à la suppression)
            modelBuilder.Entity<ModuleCours>()
                .HasRequired(m => m.Cours)            //  un module appartient à un cours (FK requise)
                .WithMany(c => c.Modules)             //un cours possède plusieurs modules
                .HasForeignKey(m => m.CoursId)        //  clé étrangère = ModuleCours.CoursId
                .WillCascadeOnDelete(false);          // la suppression du cours ne supprime pas les modules

          
            modelBuilder.Entity<Ressource>()
                .HasRequired(r => r.ModuleCours)
                .WithMany(m => m.Ressources)
                .HasForeignKey(r => r.ModuleCoursId)
                .WillCascadeOnDelete(false);

            
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Cours)
                .WithMany(c => c.Questions)
                .HasForeignKey(q => q.CoursId)
                .WillCascadeOnDelete(false);

          
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Ressource)
                .WithMany(r => r.Questions)
                .HasForeignKey(q => q.RessourceId)
                .WillCascadeOnDelete(false);

          
            modelBuilder.Entity<Question>()
                .HasRequired(q => q.Etudiant)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.EtudiantId)
                .WillCascadeOnDelete(false);

            
            modelBuilder.Entity<Reponse>()
                .HasRequired(r => r.Question)
                .WithMany(q => q.Reponses)
                .HasForeignKey(r => r.QuestionId)
                .WillCascadeOnDelete(true);

            
            modelBuilder.Entity<Reponse>()
                .HasRequired(r => r.Tuteur)
                .WithMany(u => u.Reponses)
                .HasForeignKey(r => r.TuteurId)
                .WillCascadeOnDelete(false);

            
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

           
            modelBuilder.Entity<Paiement>()
                .HasRequired(p => p.Utilisateur)
                .WithMany(u => u.Paiements)
                .HasForeignKey(p => p.UtilisateurId)
                .WillCascadeOnDelete(false);

            
            modelBuilder.Entity<SessionClavardage>()
                .HasRequired(s => s.Etudiant)
                .WithMany()
                .HasForeignKey(s => s.EtudiantId)
                .WillCascadeOnDelete(false);

            
            modelBuilder.Entity<SessionClavardage>()
                .HasRequired(s => s.Tuteur)
                .WithMany()
                .HasForeignKey(s => s.TuteurId)
                .WillCascadeOnDelete(false);
        }
    }
}
