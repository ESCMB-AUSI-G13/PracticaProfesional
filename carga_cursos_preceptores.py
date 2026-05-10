"""
Script de carga: preceptores + cursos años 2/3/4 + corrección de EspaciosCurriculares.
Nota: PreceptorId en CrearCursoDto espera el UsuarioId del preceptor.
"""
import json, urllib.request, urllib.error, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE_URL = "http://localhost:5000"

def req(method, path, body=None, token=None):
    url = f"{BASE_URL}{path}"
    data = json.dumps(body, ensure_ascii=False).encode("utf-8") if body else None
    headers = {"Content-Type": "application/json; charset=utf-8"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    r = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(r) as resp:
            content = resp.read().decode("utf-8")
            return resp.status, json.loads(content) if content else {}
    except urllib.error.HTTPError as e:
        content = e.read().decode("utf-8")
        return e.code, json.loads(content) if content else {}

def login():
    s, b = req("POST", "/api/auth/login", {"email": "admin@institucion.edu.ar", "password": "Admin1234!"})
    if s == 200:
        print("Login OK")
        return b["token"]
    raise Exception(f"Login fallido: {b}")

# ── IDs de materias por año (según BD) ─────────────────────────────────────
# Año 0 y 1: ya están correctamente asignados a Cursos 1 y 2 → no tocar
MATERIAS_ANIO2 = [25, 26, 27, 28, 29, 30, 31, 49, 50,   # Carrera 1 año 2
                  12, 13, 14, 15, 16]                     # Carrera 2 año 2
MATERIAS_ANIO3 = [32, 33, 34, 35, 36, 37, 38, 39, 51]   # Carrera 1 año 3
MATERIAS_ANIO4 = [40, 41, 42, 43, 44, 45, 46, 47, 52, 53]  # Carrera 1 año 4

UPPER_YEAR_MATS = set(MATERIAS_ANIO2 + MATERIAS_ANIO3 + MATERIAS_ANIO4)

# Docente usuarioId por materia (obtenido de la carga anterior)
DOCENTE_POR_MATERIA = {
    25: 27,  # Diego Fernández      → Historia Social y Económica
    26: 25,  # Valeria Torres       → Psicología y Educación
    27: 23,  # Patricia González    → Didáctica General
    28: 29,  # Nicolás Suárez       → Microeconomía
    29: 22,  # Ana López            → SIC Superior
    30: 30,  # Alejandra Vega       → Matemática Financiera
    31: 34,  # Sergio Romero        → CC Económicas Didáctica I
    49: 28,  # Beatriz Herrera      → Práctica Docente II
    50: 25,  # Valeria Torres       → Sujetos de la Educación
    12: 44,  # Marcela Quiroga      → Espacio Orientado II
    13: 44,  # Marcela Quiroga      → Espacio Orientado III
    14: 43,  # Pablo Delgado        → Sujetos y procesos
    15: 42,  # Silvana Miranda      → Práctica Docente II C2
    16: 45,  # Tomás Ruiz           → Proyectos Integrados
    32: 27,  # Diego Fernández      → Historia del pensamiento económico
    33: 30,  # Alejandra Vega       → Probabilidad y estadística
    34: 33,  # Gustavo Castro       → Historia y política educación Arg
    35: 32,  # Laura Morales        → Filosofía y Educación
    36: 29,  # Nicolás Suárez       → Macroeconomía
    37: 19,  # Juan Rodríguez       → Administración y Organización
    38: 31,  # Carlos Ramírez       → Costos y análisis
    39: 34,  # Sergio Romero        → CC Económicas Didáctica II
    51: 28,  # Beatriz Herrera      → Práctica Docente III
    40: 32,  # Laura Morales        → Ética y construcción de Ciudadanía
    41: 33,  # Gustavo Castro       → Problemáticas Nivel Secundario
    42: 35,  # Marcela Acosta       → Economía y Desarrollo Sustentable
    43: 31,  # Carlos Ramírez       → Teoría y Práctica Impositiva
    44: 35,  # Marcela Acosta       → Economía Financiera
    45: 35,  # Marcela Acosta       → Economía del Sector Público
    46: 21,  # Roberto Martínez     → Derecho Civil y Comercial
    47: 36,  # Fernando Ríos        → Derecho Laboral
    52: 36,  # Fernando Ríos        → Práctica Docente IV y Residencia
    53: 37,  # Amanda Flores        → UDI Digital
}

# ── Plan de preceptores nuevos ──────────────────────────────────────────────
# (nombre, apellido, dni, legajo, email, telefono, turno, anioLectivo, comision)
PRECEPTORES_PLAN = [
    ("Laura Vanesa",   "Soria",    "29456012", "PREC-003", "laura.soria@institucion.edu.ar",     "3413030001", "Mañana", 2, "A"),
    ("Ramiro Ezequiel","Gómez",    "31567890", "PREC-004", "ramiro.gomez@institucion.edu.ar",    "3413030002", "Tarde",  2, "B"),
    ("Daniela Cecilia","Rueda",    "27678901", "PREC-005", "daniela.rueda@institucion.edu.ar",   "3413030003", "Mañana", 3, "A"),
    ("Pablo Agustín",  "Torres",   "33789012", "PREC-006", "pablo.torres.pr@institucion.edu.ar","3413030004", "Tarde",  3, "B"),
    ("Valeria Noemí",  "Castillo", "28890123", "PREC-007", "valeria.castillo@institucion.edu.ar","3413030005","Mañana", 4, "A"),
    ("Martín Leandro", "Ferreira", "35901234", "PREC-008", "martin.ferreira@institucion.edu.ar", "3413030006","Tarde",  4, "B"),
]

def crear_preceptor(token, datos, preceptores_existentes):
    s, b = req("POST", "/api/preceptores", datos, token)
    if s == 201:
        print(f"  OK Preceptor: {datos['Nombre']} {datos['Apellido']} → usuarioId={b['usuarioId']}")
        return b["usuarioId"]
    elif s == 409:
        # Buscar en los existentes por legajo
        match = next((p for p in preceptores_existentes if p["legajo"] == datos["Legajo"]), None)
        if match:
            print(f"  ~ Ya existe: {datos['Apellido']} → usuarioId={match['usuarioId']}")
            return match["usuarioId"]
        print(f"  ~ Ya existe pero no encontrado por legajo: {datos['Apellido']}")
        return None
    else:
        print(f"  ERR {s}: {datos['Apellido']} → {b}")
        return None

def crear_curso(token, anio, anio_lectivo, comision, cupo, preceptor_usuario_id, cursos_existentes):
    s, b = req("POST", "/api/cursos", {
        "Anio":        anio,
        "AnioLectivo": anio_lectivo,
        "Comision":    comision,
        "Cupo":        cupo,
        "PreceptorId": preceptor_usuario_id
    }, token)
    if s == 201:
        print(f"  OK Curso: Año {anio_lectivo} Comision {comision} → cursoId={b['id']}")
        return b["id"]
    elif s in (400, 409):
        match = next((c for c in cursos_existentes
                      if c["anioLectivo"] == anio_lectivo and c["comision"] == comision), None)
        if match:
            print(f"  ~ Ya existe: Año {anio_lectivo} Comision {comision} → cursoId={match['id']}")
            return match["id"]
        print(f"  ~ Ya existe pero no hallado en listado: Año {anio_lectivo} Comision {comision}")
        return None
    else:
        print(f"  ERR {s}: Año {anio_lectivo} Comision {comision} → {b}")
        return None

def asignar_ec(token, usuario_docente_id, materia_id, curso_id, label):
    s, b = req("POST", "/api/espacios-curriculares", {
        "MateriaId":        materia_id,
        "DocenteId":        usuario_docente_id,
        "UsuarioDocenteId": usuario_docente_id,
        "CursoId":          curso_id
    }, token)
    if s == 201:
        print(f"    OK: {b['materiaNombre'][:40]} | {label}")
    elif s == 409:
        print(f"    ~ Ya existe: MatId={materia_id} | {label}")
    else:
        print(f"    ERR {s}: MatId={materia_id} | {label} → {b}")

def main():
    token = login()

    # ── 1. Eliminar ECs de materias de años 2/3/4 que están en cursos de 1er año ──
    print("\n── Eliminando ECs mal ubicados (años 2/3/4 en curso de 1er año) ──")
    s, ecs = req("GET", "/api/espacios-curriculares", token=token)
    eliminados = 0
    for ec in ecs:
        if ec["materiaId"] in UPPER_YEAR_MATS:
            s2, _ = req("DELETE", f"/api/espacios-curriculares/{ec['id']}", token=token)
            if s2 in (200, 204):
                print(f"  Eliminado EC.id={ec['id']} (MatId={ec['materiaId']} | {ec['materiaNombre'][:35]})")
                eliminados += 1
    print(f"  Total eliminados: {eliminados}")

    # ── 2. Crear preceptores y cursos ──────────────────────────────────────────────
    print("\n── Creando preceptores y cursos (años 2, 3, 4) ──")
    curso_ids = {}  # (anio_lectivo, comision) → cursoId

    _, preceptores_existentes = req("GET", "/api/preceptores", token=token)
    _, cursos_existentes = req("GET", "/api/cursos", token=token)

    for nombre, apellido, dni, legajo, email, tel, turno, anio_lect, comision in PRECEPTORES_PLAN:
        usuario_id = crear_preceptor(token, {
            "DNI":      dni,
            "Legajo":   legajo,
            "Email":    email,
            "Nombre":   nombre,
            "Apellido": apellido,
            "Password": "Preceptor2024!",
            "Telefono": tel,
            "Turno":    turno
        }, preceptores_existentes)
        if usuario_id:
            curso_id = crear_curso(token, 2026, anio_lect, comision, 30, usuario_id, cursos_existentes)
            if curso_id:
                curso_ids[(anio_lect, comision)] = curso_id

    print(f"\n  Cursos creados: {curso_ids}")

    # ── 3. Crear ECs correctos para años 2/3/4 en ambas comisiones ────────────────
    print("\n── Asignando materias a los nuevos cursos ──")

    grupos = [
        (2, MATERIAS_ANIO2),
        (3, MATERIAS_ANIO3),
        (4, MATERIAS_ANIO4),
    ]

    for anio_lect, materias in grupos:
        id_a = curso_ids.get((anio_lect, "A"))
        id_b = curso_ids.get((anio_lect, "B"))
        print(f"\n  Año {anio_lect}: cursoA={id_a}, cursoB={id_b}")

        for mat_id in materias:
            doc_uid = DOCENTE_POR_MATERIA.get(mat_id)
            if not doc_uid:
                print(f"    WARN: sin docente para MatId={mat_id}")
                continue
            if id_a:
                asignar_ec(token, doc_uid, mat_id, id_a, f"Año {anio_lect} Com.A")
            if id_b:
                asignar_ec(token, doc_uid, mat_id, id_b, f"Año {anio_lect} Com.B")

    print("\nCarga completada.")

if __name__ == "__main__":
    main()
