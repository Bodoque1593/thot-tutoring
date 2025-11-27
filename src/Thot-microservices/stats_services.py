"""
role:
- Microservice de statistiques (FastAPI)
- Calcule un aperçu global sur la base ThotDb
"""

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List
import pyodbc

# -------------------------------------------------------------------
# Connexion SQL Server
# -------------------------------------------------------------------

CONN_STR = (
    "Driver={ODBC Driver 17 for SQL Server};"
    "Server=BODOQUE-TUF\\JUAN2;"
    "Database=ThotDb;"
    "Trusted_Connection=yes;"
)


def get_connection():
    try:
        return pyodbc.connect(CONN_STR)
    except Exception as ex:
        print("Erreur connexion SQL:", ex)
        raise HTTPException(status_code=500, detail="Erreur de connexion SQL")


# -------------------------------------------------------------------
# Modèles Pydantic
# -------------------------------------------------------------------

class CourseStat(BaseModel):
    id: int
    nom: str
    nbInscriptions: int


class StatsOverview(BaseModel):
    totalCours: int
    totalUtilisateurs: int
    totalQuestions: int
    questionsOuvertes: int
    totalFaq: int
    topCours: List[CourseStat]


# -------------------------------------------------------------------
# App FastAPI
# -------------------------------------------------------------------

app = FastAPI(
    title="Thot - Microservice Statistiques",
    description="Microservice qui expose des statistiques globales basées sur la BD Thot.",
    version="1.0.0",
)


@app.get("/health")
def health():
    return {"status": "ok", "service": "stats"}


@app.get("/stats/overview", response_model=StatsOverview)
def stats_overview():
    """
    Retourne des statistiques globales :
      - totalCours
      - totalUtilisateurs
      - totalQuestions
      - questionsOuvertes (Questions non résolues)
      - totalFaq
      - topCours (Top 5 cours par nombre d'inscriptions)
    """
    try:
        conn = get_connection()
        cur = conn.cursor()

        cur.execute("SELECT COUNT(*) FROM dbo.Cours;")
        total_cours = cur.fetchone()[0]

        cur.execute("SELECT COUNT(*) FROM dbo.Utilisateurs;")
        total_utilisateurs = cur.fetchone()[0]

        cur.execute("SELECT COUNT(*) FROM dbo.Questions;")
        total_questions = cur.fetchone()[0]

        cur.execute("SELECT COUNT(*) FROM dbo.Questions WHERE EstResolvee = 0;")
        questions_ouvertes = cur.fetchone()[0]

        cur.execute("SELECT COUNT(*) FROM dbo.EntreeFAQs;")
        total_faq = cur.fetchone()[0]

        cur.execute(
            """
            SELECT TOP 5
                c.id,
                c.Nom,
                COUNT(i.id) AS NbInscrits
            FROM dbo.Cours c
            LEFT JOIN dbo.Inscriptions i ON i.CoursId = c.id
            GROUP BY c.id, c.Nom
            ORDER BY NbInscrits DESC, c.Nom ASC;
            """
        )

        top_cours_rows = cur.fetchall()
        top_cours: List[CourseStat] = []

        for row in top_cours_rows:
            top_cours.append(
                CourseStat(
                    id=row[0],
                    nom=row[1],
                    nbInscriptions=row[2],
                )
            )

        cur.close()
        conn.close()

        return StatsOverview(
            totalCours=total_cours,
            totalUtilisateurs=total_utilisateurs,
            totalQuestions=total_questions,
            questionsOuvertes=questions_ouvertes,
            totalFaq=total_faq,
            topCours=top_cours,
        )

    except HTTPException:
        raise
    except Exception as ex:
        print("Erreur stats_overview:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne stats")
