"""
role:
- Microservice d'inscriptions (FastAPI)
- Gère l'inscription d'un utilisateur à un cours (dbo.Inscriptions)
"""

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List
from datetime import datetime
import pyodbc

app = FastAPI(
    title="Thot - Microservice Inscription",
    description="Gestion des inscriptions (SQL Server) pour Thot",
    version="1.0.0",
)

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


class EnrollRequest(BaseModel):
    UtilisateurId: int
    CoursId: int


class InscriptionOut(BaseModel):
    id: int
    UtilisateurId: int
    CoursId: int
    InscritLe: datetime


@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/inscriptions/enroll", response_model=InscriptionOut)
def enroll(req: EnrollRequest):
    """
    Inscrit un utilisateur à un cours, si non déjà inscrit.
    """
    if req.UtilisateurId <= 0 or req.CoursId <= 0:
        raise HTTPException(status_code=422, detail="Ids invalides.")

    try:
        conn = get_connection()
        cur = conn.cursor()

        # Vérifier si le cours existe
        cur.execute("SELECT COUNT(*) FROM dbo.Cours WHERE id = ?;", req.CoursId)
        row = cur.fetchone()
        if not row or row[0] == 0:
            cur.close()
            conn.close()
            raise HTTPException(status_code=404, detail="Cours introuvable.")

        # Vérifier si déjà inscrit
        cur.execute(
            """
            SELECT id, UtilisateurId, CoursId, InscritLe
            FROM dbo.Inscriptions
            WHERE UtilisateurId = ? AND CoursId = ?;
            """,
            req.UtilisateurId,
            req.CoursId,
        )
        row = cur.fetchone()
        if row:
            cur.close()
            conn.close()
            return InscriptionOut(
                id=row[0],
                UtilisateurId=row[1],
                CoursId=row[2],
                InscritLe=row[3],
            )

        # Créer une nouvelle inscription
        cur.execute(
            """
            INSERT INTO dbo.Inscriptions (UtilisateurId, CoursId, InscritLe)
            OUTPUT INSERTED.id, INSERTED.UtilisateurId, INSERTED.CoursId, INSERTED.InscritLe
            VALUES (?, ?, SYSUTCDATETIME());
            """,
            req.UtilisateurId,
            req.CoursId,
        )
        row = cur.fetchone()
        conn.commit()
        cur.close()
        conn.close()

        if not row:
            raise HTTPException(
                status_code=500,
                detail="Inscription créée mais non retrouvée.",
            )

        return InscriptionOut(
            id=row[0],
            UtilisateurId=row[1],
            CoursId=row[2],
            InscritLe=row[3],
        )

    except HTTPException:
        raise
    except Exception as ex:
        print("Erreur /inscriptions/enroll:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne inscriptions")
