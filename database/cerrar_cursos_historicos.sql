-- Cierra todos los cursos cuyo año académico ya pasó.
-- Solo quedan Activos los del año en curso (2026).
UPDATE Cursos
SET    Estado = 'Cerrado'
WHERE  Anio < YEAR(GETDATE());

-- Verificación
SELECT Anio, Estado, COUNT(*) AS Cantidad
FROM   Cursos
GROUP BY Anio, Estado
ORDER BY Anio;
