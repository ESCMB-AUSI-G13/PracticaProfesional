-- ============================================================
-- LIMPIEZA SELECTIVA — Sistema Académico Integral
--
-- QUÉ BORRA:
--   Estudiantes, Cursos y lo que tiene FK NOT NULL hacia ellos:
--   inscripciones, exámenes, notas, asistencias, historial,
--   espacios curriculares, encuestas y sus respuestas.
--
-- QUÉ CONSERVA (sin modificar):
--   Docentes, Preceptores, Carreras, Materias, Correlatividades
--   Usuarios (Docentes, Preceptores, Dirección)
--   AuditoriaLogs, AuditoriaCambiosRol, LogsSeguridad
--   CalendarioAcademico (sin cambios)
--
-- QUÉ AJUSTA (NULLifica referencias huérfanas):
--   AuditoriaCambios  → pone NULL en ExamenId, InscripcionExamenId, InscripcionMateriaId
--   Alertas           → se eliminan solo las de alumnos (EstudianteId NOT NULL)
--   Notificaciones    → se eliminan solo las de usuarios alumnos
--
-- EJECUTAR en SQL Server Management Studio o Azure Data Studio.
-- ============================================================

BEGIN TRANSACTION;
BEGIN TRY

    -- ── 1. Encuestas y todo lo relacionado (no tienen FK a Estudiantes) ───────
    DELETE FROM EncuestasCompletadas;
    DELETE FROM ItemsRespuesta;
    DELETE FROM RespuestasEncuesta;
    DELETE FROM PreguntasEncuesta;
    DELETE FROM Encuestas;
    PRINT 'Encuestas y respuestas: OK';

    -- ── 2. Inscripciones a exámenes (FK NOT NULL a Estudiantes y Examenes) ────
    DELETE FROM InscripcionesExamen;
    PRINT 'InscripcionesExamen: OK';

    -- ── 3. Inscripciones a materias (FK NOT NULL a Estudiantes, Materias, Cursos)
    DELETE FROM InscripcionesMateria;
    PRINT 'InscripcionesMateria: OK';

    -- ── 4. Asistencias (FK NOT NULL a Estudiantes, Materias, Cursos) ──────────
    DELETE FROM Asistencias;
    PRINT 'Asistencias: OK';

    -- ── 5. Historial académico (FK NOT NULL a Estudiantes, Materias, Cursos) ──
    DELETE FROM HistorialAcademico;
    PRINT 'HistorialAcademico: OK';

    -- ── 6. Espacios curriculares (FK NOT NULL a Cursos — obligatorio borrar) ──
    DELETE FROM EspaciosCurriculares;
    PRINT 'EspaciosCurriculares: OK';

    -- ── 7. Exámenes (FK NOT NULL a Materias — se borran pero las materias quedan)
    DELETE FROM Examenes;
    PRINT 'Examenes: OK';

    -- ── 8. Cursos (FK NOT NULL a Preceptores) ────────────────────────────────
    DELETE FROM Cursos;
    PRINT 'Cursos: OK';

    -- ── 9. Estudiantes (FK NOT NULL a Usuarios) ──────────────────────────────
    DELETE FROM Estudiantes;
    PRINT 'Estudiantes: OK';

    -- ── 10. Usuarios con Rol = 'Estudiante' ──────────────────────────────────
    DELETE FROM Usuarios WHERE Rol = 'Estudiante';
    PRINT 'Usuarios Estudiantes: OK';

    -- ── 11. Notificaciones: solo las de alumnos (FK a UsuarioId) ─────────────
    --   Las notificaciones de Docentes/Preceptores/Dirección se conservan.
    DELETE FROM Notificaciones
    WHERE UsuarioId NOT IN (
        SELECT Id FROM Usuarios WHERE Rol IN ('Docente', 'Preceptor', 'Direccion')
    );
    PRINT 'Notificaciones de alumnos: OK';

    -- ── 12. Alertas: solo las vinculadas a estudiantes (EstudianteId no nulo) ─
    DELETE FROM Alertas WHERE EstudianteId IS NOT NULL;
    PRINT 'Alertas de alumnos: OK';

    -- ── 13. AuditoriaCambios: NULLificar referencias huérfanas (FKs nullable) ─
    --   Se conservan los registros pero se limpian las referencias a entidades borradas.
    UPDATE AuditoriaCambios
    SET    ExamenId             = NULL,
           InscripcionExamenId  = NULL,
           InscripcionMateriaId = NULL
    WHERE  ExamenId             IS NOT NULL
        OR InscripcionExamenId  IS NOT NULL
        OR InscripcionMateriaId IS NOT NULL;
    PRINT 'AuditoriaCambios: referencias huérfanas NULLificadas, registros conservados';

    COMMIT TRANSACTION;
    PRINT '============================================================';
    PRINT 'Limpieza completada correctamente.';
    PRINT 'Conservados: Docentes, Preceptores, Materias, Correlatividades,';
    PRINT '             Carreras, CalendarioAcademico, AuditoriaCambios (sin refs).';
    PRINT '============================================================';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR — se revirtió todo el proceso:';
    PRINT ERROR_MESSAGE();
END CATCH;
