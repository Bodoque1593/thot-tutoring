namespace Thot_projet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PremierBase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Abonnements",
                c => new
                    {
                        UtilisateurId = c.Int(nullable: false),
                        Type = c.String(nullable: false),
                        DebutLe = c.DateTime(),
                        ExpireLe = c.DateTime(),
                        Actif = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.UtilisateurId)
                .ForeignKey("dbo.Utilisateurs", t => t.UtilisateurId)
                .Index(t => t.UtilisateurId);
            
            CreateTable(
                "dbo.Utilisateurs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Nomcomplet = c.String(),
                        Email = c.String(),
                        Role = c.String(),
                        Creele = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Inscriptions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        UtilisateurId = c.Int(nullable: false),
                        CoursId = c.Int(nullable: false),
                        InscritLe = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Cours", t => t.CoursId)
                .ForeignKey("dbo.Utilisateurs", t => t.UtilisateurId)
                .Index(t => t.UtilisateurId)
                .Index(t => t.CoursId);
            
            CreateTable(
                "dbo.Cours",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Nom = c.String(),
                        Niveau = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.ModuleCours",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        CoursId = c.Int(nullable: false),
                        Numero = c.Int(nullable: false),
                        Titre = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Cours", t => t.CoursId)
                .Index(t => t.CoursId);
            
            CreateTable(
                "dbo.Ressources",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        ModuleCoursId = c.Int(nullable: false),
                        Type = c.String(),
                        Titre = c.String(),
                        url = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.ModuleCours", t => t.ModuleCoursId)
                .Index(t => t.ModuleCoursId);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        EtudiantId = c.Int(nullable: false),
                        CoursId = c.Int(nullable: false),
                        RessourceId = c.Int(nullable: false),
                        Contenu = c.String(),
                        EstResolvee = c.Boolean(nullable: false),
                        Creele = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Cours", t => t.CoursId)
                .ForeignKey("dbo.Utilisateurs", t => t.EtudiantId)
                .ForeignKey("dbo.Ressources", t => t.RessourceId)
                .Index(t => t.EtudiantId)
                .Index(t => t.CoursId)
                .Index(t => t.RessourceId);
            
            CreateTable(
                "dbo.Reponses",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        QuestionId = c.Int(nullable: false),
                        TuteurId = c.Int(nullable: false),
                        Contenu = c.String(),
                        Creele = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .ForeignKey("dbo.Utilisateurs", t => t.TuteurId)
                .Index(t => t.QuestionId)
                .Index(t => t.TuteurId);
            
            CreateTable(
                "dbo.Paiements",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        UtilisateurId = c.Int(nullable: false),
                        Montant = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Monnaie = c.String(),
                        Statut = c.String(),
                        PayeLe = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Utilisateurs", t => t.UtilisateurId)
                .Index(t => t.UtilisateurId);
            
            CreateTable(
                "dbo.ProfilTuteurs",
                c => new
                    {
                        UtilisateurId = c.Int(nullable: false),
                        Sujets = c.String(),
                        Niveaux = c.String(),
                    })
                .PrimaryKey(t => t.UtilisateurId)
                .ForeignKey("dbo.Utilisateurs", t => t.UtilisateurId)
                .Index(t => t.UtilisateurId);
            
            CreateTable(
                "dbo.EntreeFAQs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        QuestionTexte = c.String(),
                        ReponseTexte = c.String(),
                        PublieLe = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.SessionClavardages",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        EtudiantId = c.Int(nullable: false),
                        TuteurId = c.Int(nullable: false),
                        DemarreLe = c.DateTime(nullable: false),
                        TermineLe = c.DateTime(),
                        DureeMinutes = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Utilisateurs", t => t.EtudiantId)
                .ForeignKey("dbo.Utilisateurs", t => t.TuteurId)
                .Index(t => t.EtudiantId)
                .Index(t => t.TuteurId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SessionClavardages", "TuteurId", "dbo.Utilisateurs");
            DropForeignKey("dbo.SessionClavardages", "EtudiantId", "dbo.Utilisateurs");
            DropForeignKey("dbo.Abonnements", "UtilisateurId", "dbo.Utilisateurs");
            DropForeignKey("dbo.ProfilTuteurs", "UtilisateurId", "dbo.Utilisateurs");
            DropForeignKey("dbo.Paiements", "UtilisateurId", "dbo.Utilisateurs");
            DropForeignKey("dbo.Inscriptions", "UtilisateurId", "dbo.Utilisateurs");
            DropForeignKey("dbo.Inscriptions", "CoursId", "dbo.Cours");
            DropForeignKey("dbo.Questions", "RessourceId", "dbo.Ressources");
            DropForeignKey("dbo.Reponses", "TuteurId", "dbo.Utilisateurs");
            DropForeignKey("dbo.Reponses", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.Questions", "EtudiantId", "dbo.Utilisateurs");
            DropForeignKey("dbo.Questions", "CoursId", "dbo.Cours");
            DropForeignKey("dbo.Ressources", "ModuleCoursId", "dbo.ModuleCours");
            DropForeignKey("dbo.ModuleCours", "CoursId", "dbo.Cours");
            DropIndex("dbo.SessionClavardages", new[] { "TuteurId" });
            DropIndex("dbo.SessionClavardages", new[] { "EtudiantId" });
            DropIndex("dbo.ProfilTuteurs", new[] { "UtilisateurId" });
            DropIndex("dbo.Paiements", new[] { "UtilisateurId" });
            DropIndex("dbo.Reponses", new[] { "TuteurId" });
            DropIndex("dbo.Reponses", new[] { "QuestionId" });
            DropIndex("dbo.Questions", new[] { "RessourceId" });
            DropIndex("dbo.Questions", new[] { "CoursId" });
            DropIndex("dbo.Questions", new[] { "EtudiantId" });
            DropIndex("dbo.Ressources", new[] { "ModuleCoursId" });
            DropIndex("dbo.ModuleCours", new[] { "CoursId" });
            DropIndex("dbo.Inscriptions", new[] { "CoursId" });
            DropIndex("dbo.Inscriptions", new[] { "UtilisateurId" });
            DropIndex("dbo.Abonnements", new[] { "UtilisateurId" });
            DropTable("dbo.SessionClavardages");
            DropTable("dbo.EntreeFAQs");
            DropTable("dbo.ProfilTuteurs");
            DropTable("dbo.Paiements");
            DropTable("dbo.Reponses");
            DropTable("dbo.Questions");
            DropTable("dbo.Ressources");
            DropTable("dbo.ModuleCours");
            DropTable("dbo.Cours");
            DropTable("dbo.Inscriptions");
            DropTable("dbo.Utilisateurs");
            DropTable("dbo.Abonnements");
        }
    }
}
