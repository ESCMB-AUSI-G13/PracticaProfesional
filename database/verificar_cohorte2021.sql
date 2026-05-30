-- ============================================================
-- VERIFICACIÓN COHORTE 2021
-- ============================================================

-- 1. Resumen de estudiantes por carrera y condición
SELECT
    e.CarreraId,
    c.Nombre                              AS Carrera,
    e.Condicion,
    COUNT(*)                              AS Cantidad
FROM  Estudiantes e
JOIN  Carreras    c ON c.Id = e.CarreraId
WHERE YEAR(e.FechaDeIngreso) = 2021
GROUP BY e.CarreraId, c.Nombre, e.Condicion
ORDER BY e.CarreraId, e.Condicion;

-- 2. Total general cohorte 2021
SELECT COUNT(*) AS TotalEstudiantes2021
FROM  Estudiantes
WHERE YEAR(FechaDeIngreso) = 2021;

-- 3. Inscripciones a materias de cursos 2021
SELECT
    cu.CarreraId,
    cu.Comision,
    im.Estado,
    COUNT(*) AS Inscripciones
FROM  InscripcionesMateria im
JOIN  Cursos               cu ON cu.Id = im.CursoId
WHERE cu.Anio = 2021
GROUP BY cu.CarreraId, cu.Comision, im.Estado
ORDER BY cu.CarreraId, cu.Comision, im.Estado;

-- 4. Total inscripciones 2021
SELECT COUNT(*) AS TotalInscripciones2021
FROM  InscripcionesMateria im
JOIN  Cursos               cu ON cu.Id = im.CursoId
WHERE cu.Anio = 2021;

-- 5. Verificar FechaInscripcion histórica
SELECT TOP 5
    im.FechaInscripcion,
    im.Estado,
    cu.Anio,
    cu.CarreraId,
    cu.Comision
FROM  InscripcionesMateria im
JOIN  Cursos               cu ON cu.Id = im.CursoId
WHERE cu.Anio = 2021;
