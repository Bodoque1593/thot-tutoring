from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import Optional
import os
import stripe
import pyodbc

app = FastAPI(
    title="Thot - Microservice Paiement (Stripe)",
    description="Crée des sessions de paiement Stripe pour les cours Thot.",
    version="1.0.0",
)

# -------------------------------------------------------------------
#  CONFIG : SQL Server + Stripe
#  (usa la misma conexión que ya probaste en test_db.py / faq_service)
# -------------------------------------------------------------------
SQL_CONN_STR = os.getenv(
    "SQLSERVER_CONN_STR",
    "Driver={ODBC Driver 17 for SQL Server};"
    "Server=BODOQUE-TUF\\JUAN2;"
    "Database=ThotDb;"
    "Trusted_Connection=yes;"
)

STRIPE_API_KEY = os.getenv("STRIPE_API_KEY", "sk_test_REMPLACE_MOI")

# URLs de redirección hacia tu aplicación ASP.NET (ajústalas)
STRIPE_SUCCESS_URL = os.getenv(
    "STRIPE_SUCCESS_URL",
    "http://localhost:12345/Paiement/Success?session_id={CHECKOUT_SESSION_ID}"
)
STRIPE_CANCEL_URL = os.getenv(
    "STRIPE_CANCEL_URL",
    "http://localhost:12345/Paiement/Cancel"
)

stripe.api_key = STRIPE_API_KEY


def get_connection():
    try:
        return pyodbc.connect(SQL_CONN_STR)
    except Exception as ex:
        print("❌ Erreur connexion SQL:", ex)
        raise HTTPException(status_code=500, detail="Erreur de connexion SQL")


# -------------------------------------------------------------------
#  MODELOS Pydantic
# -------------------------------------------------------------------
class CheckoutRequest(BaseModel):
    CoursId: int
    UtilisateurEmail: str
    Currency: Optional[str] = "cad"   # por defecto CAD, puedes cambiarlo


class CheckoutResponse(BaseModel):
    checkout_url: str
    session_id: str
    course_name: str
    price: float


# -------------------------------------------------------------------
#  ENDPOINTS
# -------------------------------------------------------------------

@app.get("/health", tags=["health"])
def health():
    return {"status": "ok"}


@app.post("/create-checkout-session", response_model=CheckoutResponse, tags=["paiement"])
def create_checkout(req: CheckoutRequest):
    """
    1) Lee el curso (Nom, Prix) desde dbo.Cours.
    2) Crea una Stripe Checkout Session.
    3) Devuelve la URL para redirigir al estudiante.
    """
    # 1) Leer curso en SQL Server
    try:
        conn = get_connection()
        cur = conn.cursor()
        cur.execute("SELECT Nom, Prix FROM dbo.Cours WHERE id = ?", req.CoursId)
        row = cur.fetchone()
        if not row:
            raise HTTPException(status_code=404, detail="Cours introuvable.")

        course_name = row[0]
        prix = float(row[1])  # decimal -> float
    except HTTPException:
        raise
    except Exception as ex:
        print("❌ Erreur SQL (create_checkout):", ex)
        raise HTTPException(status_code=500, detail="Erreur SQL lors de la lecture du cours.")
    finally:
        try:
            cur.close()
            conn.close()
        except:
            pass

    # 2) Crear sesión de pago en Stripe
    try:
        session = stripe.checkout.Session.create(
            mode="payment",
            customer_email=req.UtilisateurEmail,
            line_items=[
                {
                    "price_data": {
                        "currency": req.Currency,
                        "product_data": {
                            "name": f"Thot - {course_name}"
                        },
                        "unit_amount": int(prix * 100),  # Stripe usa centavos
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
        print("❌ Erreur Stripe:", ex)
        raise HTTPException(status_code=500, detail="Erreur Stripe lors de la création du paiement.")
