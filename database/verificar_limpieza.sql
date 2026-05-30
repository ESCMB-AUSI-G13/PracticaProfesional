-- ============================================================
-- VERIFICACIÓN DE LIMPIEZA SELECTIVA
-- ============================================================

SELECT 'Cursos'               AS Tabla, COUNT(*) AS Registros FROM Cursos
UNION ALL
SELECT 'Estudiantes',                   COUNT(*) FROM Estudiantes
UNION ALL
SELECT 'Usuarios (alumnos)',            COUNT(*) FROM Usuarios WHERE Rol = 'Estudiante'
UNION ALL
SELECT 'InscripcionesMateria',          COUNT(*) FROM InscripcionesMateria
UNION ALL
SELECT 'InscripcionesExamen',           COUNT(*) FROM InscripcionesExamen
UNION ALL
SELECT 'Examenes',                      COUNT(*) FROM Examenes
UNION ALL
SELECT 'Asistencias',                   COUNT(*) FROM Asistencias
UNION ALL
SELECT 'HistorialAcademico',            COUNT(*) FROM HistorialAcademico
UNION ALL
SELECT 'EspaciosCurriculares',          COUNT(*) FROM EspaciosCurriculares
UNION ALL
SELECT 'Encuestas',                     COUNT(*) FROM Encuestas
UNION ALL
SELECT '--- CONSERVADOS ---',           0
UNION ALL
SELECT 'Usuarios (no alumnos)',         COUNT(*) FROM Usuarios WHERE Rol != 'Estudiante'
UNION ALL
SELECT 'Docentes',                      COUNT(*) FROM Docentes
UNION ALL
SELECT 'Preceptores',                   COUNT(*) FROM Preceptores
UNION ALL
SELECT 'Materias',                      COUNT(*) FROM Materias
UNION ALL
SELECT 'Correlatividades',              COUNT(*) FROM Correlatividades
UNION ALL
SELECT 'Carreras',                      COUNT(*) FROM Carreras
UNION ALL
SELECT 'CalendarioAcademico',           COUNT(*) FROM CalendarioAcademico
UNION ALL
SELECT 'AuditoriaCambios',             COUNT(*) FROM AuditoriaCambios;
