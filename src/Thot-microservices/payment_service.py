"""
role:
- Microservice de paiement (FastAPI + Stripe)
- Crée une session de paiement Stripe pour un cours donné (dbo.Cours)
"""

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import Optional
import os
import stripe
import pyodbc
from dotenv import load_dotenv

# -------------------------------------------------------------------
# Chargement du .env (même dossier que ce fichier)
# -------------------------------------------------------------------

BASE_DIR = os.path.dirname(__file__)
dotenv_path = os.path.join(BASE_DIR, ".env")
load_dotenv(dotenv_path)

app = FastAPI(
    title="Thot - Microservice Paiement (Stripe)",
    description="Crée des sessions de paiement Stripe pour les cours Thot.",
    version="1.0.0",
)

# -------------------------------------------------------------------
# Connexion SQL Server
# -------------------------------------------------------------------

SQL_CONN_STR = os.getenv(
    "SQLSERVER_CONN_STR",
    "Driver={ODBC Driver 17 for SQL Server};"
    "Server=BODOQUE-TUF\\JUAN2;"
    "Database=ThotDb;"
    "Trusted_Connection=yes;",
)


def get_connection():
    try:
        return pyodbc.connect(SQL_CONN_STR)
    except Exception as ex:
        print("Erreur connexion SQL:", ex)
        raise HTTPException(status_code=500, detail=f"Erreur connexion SQL: {ex}")


# -------------------------------------------------------------------
# Configuration Stripe
# -------------------------------------------------------------------

STRIPE_SECRET_KEY = os.getenv("STRIPE_SECRET_KEY")

if not STRIPE_SECRET_KEY:
    raise RuntimeError("La variable d'environnement STRIPE_SECRET_KEY est manquante")

stripe.api_key = STRIPE_SECRET_KEY

STRIPE_SUCCESS_URL = os.getenv(
    "STRIPE_SUCCESS_URL",
    "http://localhost:44344/Paiement/Success?session_id={CHECKOUT_SESSION_ID}",
)
STRIPE_CANCEL_URL = os.getenv(
    "STRIPE_CANCEL_URL",
    "http://localhost:44344/Paiement/Cancel",
)


# -------------------------------------------------------------------
# Modèles Pydantic
# -------------------------------------------------------------------

class CheckoutRequest(BaseModel):
    CoursId: int
    UtilisateurEmail: str
    Currency: Optional[str] = "cad"


class CheckoutResponse(BaseModel):
    checkout_url: str
    session_id: str
    course_name: str
    price: float


# -------------------------------------------------------------------
# Endpoints
# -------------------------------------------------------------------

@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/create-checkout-session", response_model=CheckoutResponse)
def create_checkout(req: CheckoutRequest):
    """
    1) Lit le cours (Nom, Prix) dans dbo.Cours.
    2) Crée une session Stripe Checkout.
    3) Retourne l'URL de redirection pour l'étudiant.
    """
    try:
        conn = get_connection()
        cur = conn.cursor()
        cur.execute("SELECT Nom, Prix FROM dbo.Cours WHERE id = ?", req.CoursId)
        row = cur.fetchone()
        cur.close()
        conn.close()

        if not row:
            raise HTTPException(status_code=404, detail="Cours introuvable.")

        course_name = row[0]
        prix = float(row[1])
    except HTTPException:
        raise
    except Exception as ex:
        print("Erreur SQL (create_checkout):", ex)
        raise HTTPException(status_code=500, detail=f"Erreur SQL: {ex}")

    try:
        session = stripe.checkout.Session.create(
            mode="payment",
            customer_email=req.UtilisateurEmail,
            line_items=[
                {
                    "price_data": {
                        "currency": req.Currency,
                        "product_data": {
                            "name": f"Thot - {course_name}",
                        },
                        "unit_amount": int(prix * 100),
                    },
                    "quantity": 1,
                }
            ],
            success_url=STRIPE_SUCCESS_URL,
            cancel_url=STRIPE_CANCEL_URL,
        )

        return CheckoutResponse(
            checkout_url=session.url,
            session_id=session.id,
            course_name=course_name,
            price=prix,
        )

    except Exception as ex:
        print("Erreur Stripe:", ex)
        raise HTTPException(status_code=500, detail=f"Erreur Stripe: {ex}")
