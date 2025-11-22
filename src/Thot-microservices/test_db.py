# test_db.py
import pyodbc

CONN_STR = (
    "Driver={ODBC Driver 17 for SQL Server};"
    "Server=BODOQUE-TUF\\JUAN2;"
    "Database=ThotDb;"
    "Trusted_Connection=yes;"
)

print("Intentando conectar con:")
print(CONN_STR)

try:
    conn = pyodbc.connect(CONN_STR)
    print("✅ Conexión OK!")
    cursor = conn.cursor()
    cursor.execute("SELECT TOP 1 id, QuestionTexte FROM dbo.EntreeFAQs;")
    row = cursor.fetchone()
    print("Fila de ejemplo:", row)
    cursor.close()
    conn.close()
except Exception as ex:
    print("❌ Error de conexión:")
    print(repr(ex))
