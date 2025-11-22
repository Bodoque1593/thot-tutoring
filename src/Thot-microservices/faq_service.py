from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List
from datetime import datetime
import pyodbc

app = FastAPI(
    title="Thot - Microservice FAQ",
    description="API FAQ pour le projet Thot (SQL Server)",
    version="1.0.0",
)

# -------------------------------------------------------------------
#  CONNEXION SQL SERVER  (usa lo MISMO que ya probaste en test_db.py)
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
        print("❌ Erreur connexion SQL:", ex)
        raise HTTPException(
            status_code=500,
            detail="Erreur de connexion SQL"
        )


# -------------------------------------------------------------------
#  MODELOS Pydantic (mismos nombres que la tabla dbo.EntreeFAQs)
# -------------------------------------------------------------------
class FaqBase(BaseModel):
    QuestionTexte: str
    ReponseTexte: str


class FaqCreate(FaqBase):
    """Modelo de entrada para POST /faq"""
    pass


class FaqUpdate(FaqBase):
    """Modelo de entrada para PUT /faq/{id}"""
    pass


class FaqOut(FaqBase):
    id: int
    PublieLe: datetime

    class Config:
        orm_mode = True


# -------------------------------------------------------------------
#  ENDPOINTS
# -------------------------------------------------------------------

@app.get("/health", tags=["health"])
def health():
    return {"status": "ok"}


# LISTAR TODAS
@app.get("/faq", response_model=List[FaqOut], tags=["faq"])
def list_faq():
    """Devuelve todas las FAQ ordenadas por fecha DESC."""
    try:
        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute("""
            SELECT id, QuestionTexte, ReponseTexte, PublieLe
            FROM dbo.EntreeFAQs
            ORDER BY PublieLe DESC;
        """)

        rows = cursor.fetchall()
        faqs: List[FaqOut] = []

        for row in rows:
            faqs.append(
                FaqOut(
                    id=row[0],
                    QuestionTexte=row[1],
                    ReponseTexte=row[2],
                    PublieLe=row[3],
                )
            )

        cursor.close()
        conn.close()
        return faqs

    except HTTPException:
        raise
    except Exception as ex:
        print("❌ Erreur list_faq:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne FAQ")


# OBTENER UNA POR ID
@app.get("/faq/{faq_id}", response_model=FaqOut, tags=["faq"])
def get_faq(faq_id: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute(
            """
            SELECT id, QuestionTexte, ReponseTexte, PublieLe
            FROM dbo.EntreeFAQs
            WHERE id = ?;
            """,
            faq_id,
        )

        row = cursor.fetchone()
        cursor.close()
        conn.close()

        if not row:
            raise HTTPException(status_code=404, detail="FAQ non trouvée")

        return FaqOut(
            id=row[0],
            QuestionTexte=row[1],
            ReponseTexte=row[2],
            PublieLe=row[3],
        )

    except HTTPException:
        raise
    except Exception as ex:
        print("❌ Erreur get_faq:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne FAQ")


# CREAR (lo que ya tenías)
@app.post("/faq", response_model=FaqOut, status_code=201, tags=["faq"])
def create_faq(faq: FaqCreate):
    """
    Crea una entrada FAQ en dbo.EntreeFAQs.

    JSON esperado:
    {
      "QuestionTexte": "string",
      "ReponseTexte": "string"
    }
    """

    # Validaciones simples
    if not faq.QuestionTexte or not faq.QuestionTexte.strip():
        raise HTTPException(status_code=422, detail="QuestionTexte est requis.")
    if not faq.ReponseTexte or not faq.ReponseTexte.strip():
        raise HTTPException(status_code=422, detail="ReponseTexte est requis.")

    try:
        conn = get_connection()
        cursor = conn.cursor()

        # INSERT + OUTPUT INSERTED (tu versión que ya funciona)
        cursor.execute(
            """
            INSERT INTO dbo.EntreeFAQs (QuestionTexte, ReponseTexte, PublieLe)
            OUTPUT INSERTED.id, INSERTED.QuestionTexte, INSERTED.ReponseTexte, INSERTED.PublieLe
            VALUES (?, ?, SYSUTCDATETIME());
            """,
            faq.QuestionTexte.strip(),
            faq.ReponseTexte.strip()
        )

        row = cursor.fetchone()
        conn.commit()

        if not row:
            cursor.close()
            conn.close()
            raise HTTPException(
                status_code=500,
                detail="FAQ créée mais non retrouvée en BD."
            )

        faq_out = FaqOut(
            id=row[0],
            QuestionTexte=row[1],
            ReponseTexte=row[2],
            PublieLe=row[3],
        )

        cursor.close()
        conn.close()
        return faq_out

    except HTTPException:
        raise
    except Exception as ex:
        # VERY IMPORTANT: mira siempre la consola de VS Code para ver esto
        print("❌ Erreur create_faq:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne FAQ")


# ACTUALIZAR
@app.put("/faq/{faq_id}", response_model=FaqOut, tags=["faq"])
def update_faq(faq_id: int, faq: FaqUpdate):
    if not faq.QuestionTexte or not faq.QuestionTexte.strip():
        raise HTTPException(status_code=422, detail="QuestionTexte est requis.")
    if not faq.ReponseTexte or not faq.ReponseTexte.strip():
        raise HTTPException(status_code=422, detail="ReponseTexte est requis.")

    try:
        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute(
            """
            UPDATE dbo.EntreeFAQs
            SET QuestionTexte = ?, ReponseTexte = ?
            WHERE id = ?;
            """,
            faq.QuestionTexte.strip(),
            faq.ReponseTexte.strip(),
            faq_id,
        )

        if cursor.rowcount == 0:
            conn.rollback()
            cursor.close()
            conn.close()
            raise HTTPException(status_code=404, detail="FAQ non trouvée")

        conn.commit()

        # Volvemos a leer la fila actualizada
        cursor.execute(
            """
            SELECT id, QuestionTexte, ReponseTexte, PublieLe
            FROM dbo.EntreeFAQs
            WHERE id = ?;
            """,
            faq_id,
        )
        row = cursor.fetchone()

        cursor.close()
        conn.close()

        return FaqOut(
            id=row[0],
            QuestionTexte=row[1],
            ReponseTexte=row[2],
            PublieLe=row[3],
        )

    except HTTPException:
        raise
    except Exception as ex:
        print("❌ Erreur update_faq:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne FAQ")


# ELIMINAR
@app.delete("/faq/{faq_id}", status_code=204, tags=["faq"])
def delete_faq(faq_id: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()

        cursor.execute(
            "DELETE FROM dbo.EntreeFAQs WHERE id = ?;",
            faq_id,
        )

        if cursor.rowcount == 0:
            conn.rollback()
            cursor.close()
            conn.close()
            raise HTTPException(status_code=404, detail="FAQ non trouvée")

        conn.commit()
        cursor.close()
        conn.close()
        return

    except HTTPException:
        raise
    except Exception as ex:
        print("❌ Erreur delete_faq:", ex)
        raise HTTPException(status_code=500, detail="Erreur interne FAQ")
