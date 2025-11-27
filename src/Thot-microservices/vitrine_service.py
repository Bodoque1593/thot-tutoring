"""
role:
- Microservice Vitrine (FastAPI)
- Expose la liste des cours pour la vitrine du tuteur (lecture seule)
"""

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional
import pyodbc

app = FastAPI(
    title="Thot - Microservice Vitrine Cours",
    description="Expose la liste des cours pour la vitrine tuteur (SQL Server).",
    version="1.0.0",
)

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
        raise HTTPException(status_code=500, detail="Erreur connexion SQL")


# -------------------------------------------------------------------
# Modèle Pydantic (projection de dbo.Cours)
# -------------------------------------------------------------------

class CoursOut(BaseModel):
    id: int
    Nom: str
    Niveau: str
    Prix: float
    ImageUrl: Optional[str] = None
    Description: Optional[str] = None


# -------------------------------------------------------------------
# Endpoints
# -------------------------------------------------------------------

@app.get("/health")
def health():
    return {"status": "ok"}


@app.get("/courses", response_model=List[CoursOut])
def list_courses():
    """
    Retourne tous les cours pour la vitrine tuteur.
    """
    try:
        conn = get_connection()
        cur = conn.cursor()

        cur.execute(
            """
            SELECT id, Nom, Niveau, Prix, ImageUrl, Description
            FROM dbo.Cours
            ORDER BY Nom;
            """
        )

        rows = cur.fetchall()
        result: List[CoursOut] = []

        for r in rows:
            result.append(
                CoursOut(
                    id=r[0],
                    Nom=r[1],
                    Niveau=r[2],
                    Prix=float(r[3]),
                    ImageUrl=r[4],
                    Description=r[5],
                )
            )

        cur.close()
        conn.close()
        return result

    except HTTPException:
        raise
    except Exception as ex:
        print("Erreur list_courses:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne vitrine")


@app.get("/courses/{course_id}", response_model=CoursOut)
def get_course(course_id: int):
    """
    Retourne un cours par id (utile pour réutilisation).
    """
    try:
        conn = get_connection()
        cur = conn.cursor()

        cur.execute(
            """
            SELECT id, Nom, Niveau, Prix, ImageUrl, Description
            FROM dbo.Cours
            WHERE id = ?;
            """,
            course_id,
        )

        r = cur.fetchone()
        cur.close()
        conn.close()

        if not r:
            raise HTTPException(status_code=404, detail="Cours non trouvé")

        return CoursOut(
            id=r[0],
            Nom=r[1],
            Niveau=r[2],
            Prix=float(r[3]),
            ImageUrl=r[4],
            Description=r[5],
        )

    except HTTPException:
        raise
    except Exception as ex:
        print("Erreur get_course:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne vitrine")
