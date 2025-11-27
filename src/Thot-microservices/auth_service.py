"""
role:
- Microservice d'authentification (FastAPI)
- Gère /auth/login et /auth/register contre SQL Server
- Compatible avec le contrôleur ASP.NET MVC (mots de passe en clair)
"""

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, EmailStr
from typing import Literal
import pyodbc

app = FastAPI(
    title="Thot - Microservice Auth",
    description="Microservice pour LOGIN et REGISTER avec SQL Server",
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

# Nom réel de la table des utilisateurs
USERS_TABLE = "dbo.Utilisateurs"


def get_connection():
    try:
        return pyodbc.connect(CONN_STR)
    except Exception as ex:
        print("Erreur connexion SQL:", ex)
        raise HTTPException(status_code=500, detail="Erreur connexion SQL")


# -------------------------------------------------------------------
# Modèles Pydantic
# -------------------------------------------------------------------

class LoginRequest(BaseModel):
    Email: EmailStr
    Motdepasse: str


class RegisterRequest(BaseModel):
    Email: EmailStr
    Nomcomplet: str
    Role: Literal["Etudiant", "Tuteur"]
    Motdepasse: str


class UserOut(BaseModel):
    id: int
    Email: str
    Nomcomplet: str
    Role: str


# -------------------------------------------------------------------
# Endpoints
# -------------------------------------------------------------------

@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/auth/login", response_model=UserOut)
def login_user(req: LoginRequest):
    """
    Vérifie Email + Motdepasse dans la table des utilisateurs.
    Mot de passe en clair (comme dans AuthController ASP.NET).
    """
    conn = get_connection()
    cur = conn.cursor()

    try:
        sql = f"""
            SELECT id, Email, Nomcomplet, Role
            FROM {USERS_TABLE}
            WHERE Email = ? AND Motdepasse = ?;
        """
        cur.execute(sql, req.Email, req.Motdepasse)
        row = cur.fetchone()

        if not row:
            raise HTTPException(status_code=401, detail="Identifiants incorrects")

        return UserOut(
            id=row[0],
            Email=row[1],
            Nomcomplet=row[2],
            Role=row[3],
        )

    finally:
        cur.close()
        conn.close()


@app.post("/auth/register", response_model=UserOut, status_code=201)
def register_user(req: RegisterRequest):
    """
    Crée un utilisateur si l'email n'existe pas encore.
    Motdepasse en clair pour rester compatible avec ASP.NET.
    """
    conn = get_connection()
    cur = conn.cursor()

    try:
        # Vérifier si l’email existe déjà
        sql_check = f"SELECT id FROM {USERS_TABLE} WHERE Email = ?;"
        cur.execute(sql_check, req.Email)

        if cur.fetchone():
            raise HTTPException(status_code=409, detail="Email déjà utilisé")

        # Insérer l'utilisateur
        sql_insert = f"""
            INSERT INTO {USERS_TABLE} (Email, Nomcomplet, Role, Motdepasse, Creele)
            OUTPUT INSERTED.id, INSERTED.Email, INSERTED.Nomcomplet, INSERTED.Role
            VALUES (?, ?, ?, ?, GETUTCDATE());
        """
        cur.execute(
            sql_insert,
            req.Email,
            req.Nomcomplet,
            req.Role,
            req.Motdepasse,
        )

        row = cur.fetchone()
        conn.commit()

        return UserOut(
            id=row[0],
            Email=row[1],
            Nomcomplet=row[2],
            Role=row[3],
        )

    finally:
        cur.close()
        conn.close()
