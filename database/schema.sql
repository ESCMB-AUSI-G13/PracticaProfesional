-- ============================================================
-- Sistema Académico Integral — Script de creación de tablas
-- Motor: SQL Server
-- ============================================================

-- ──────────────────────────────────────────────────────────
-- USUARIOS Y ROLES
-- ──────────────────────────────────────────────────────────

CREATE TABLE Usuarios (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    DNI           NVARCHAR(10)  NOT NULL,
    Legajo        NVARCHAR(20)  NOT NULL,
    Email         NVARCHAR(150) NOT NULL,
    Nombre        NVARCHAR(100) NOT NULL,
    Apellido      NVARCHAR(100) NOT NULL,
    PasswordHash  NVARCHAR(MAX) NOT NULL,
    Rol           NVARCHAR(50)  NOT NULL,
    Activo        BIT           NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    PasswordResetToken        NVARCHAR(100) NULL,
    PasswordResetTokenExpiry  DATETIME2     NULL,
    CONSTRAINT UQ_Usuarios_DNI    UNIQUE (DNI),
    CONSTRAINT UQ_Usuarios_Legajo UNIQUE (Legajo),
    CONSTRAINT UQ_Usuarios_Email  UNIQUE (Email)
);

CREATE TABLE Docentes (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT          NOT NULL,
    Telefono  NVARCHAR(20) NOT NULL,
    Categoria NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_Docentes_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT UQ_Docentes_UsuarioId UNIQUE (UsuarioId)
);

CREATE TABLE Preceptores (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT          NOT NULL,
    Telefono  NVARCHAR(20) NOT NULL,
    Turno     NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Preceptores_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT UQ_Preceptores_UsuarioId UNIQUE (UsuarioId)
);

CREATE TABLE Estudiantes (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId      INT          NOT NULL,
    Anio           INT          NOT NULL,
    Condicion      NVARCHAR(50) NOT NULL,
    FechaDeIngreso DATE         NOT NULL,
    CONSTRAINT FK_Estudiantes_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT UQ_Estudiantes_UsuarioId UNIQUE (UsuarioId),
    CONSTRAINT CK_Estudiantes_Anio CHECK (Anio BETWEEN 1 AND 6)
);

-- ──────────────────────────────────────────────────────────
-- PLAN ACADÉMICO
-- ──────────────────────────────────────────────────────────

CREATE TABLE Materias (
    Id     INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(20)  NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    [Plan] NVARCHAR(20)  NOT NULL,
    CONSTRAINT UQ_Materias_Codigo UNIQUE (Codigo)
);

CREATE TABLE Correlatividades (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    MateriaDestinoId  INT          NOT NULL,
    MateriaRequisitoId INT         NOT NULL,
    TipoRequerimiento NVARCHAR(50) NOT NULL,   -- 'Cursar' | 'Rendir'
    CondicionAcademica NVARCHAR(50) NOT NULL,  -- 'Regularizado' | 'Aprobado'
    CONSTRAINT FK_Corr_Destino   FOREIGN KEY (MateriaDestinoId)  REFERENCES Materias(Id),
    CONSTRAINT FK_Corr_Requisito FOREIGN KEY (MateriaRequisitoId) REFERENCES Materias(Id),
    CONSTRAINT UQ_Correlatividades UNIQUE (MateriaDestinoId, MateriaRequisitoId, TipoRequerimiento)
);

CREATE TABLE Cursos (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Anio        INT          NOT NULL,
    Comision    NVARCHAR(20) NOT NULL,
    Cupo        INT          NOT NULL,
    Estado      NVARCHAR(20) NOT NULL DEFAULT 'Activo',
    PreceptorId INT          NOT NULL,
    CONSTRAINT FK_Cursos_Preceptores FOREIGN KEY (PreceptorId) REFERENCES Preceptores(Id),
    CONSTRAINT UQ_Cursos_AnioCom UNIQUE (Anio, Comision),
    CONSTRAINT CK_Cursos_Cupo CHECK (Cupo > 0)
);

CREATE TABLE EspaciosCurriculares (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    MateriaId  INT NOT NULL,
    DocenteId  INT NOT NULL,
    CursoId    INT NOT NULL,
    CONSTRAINT FK_EC_Materias  FOREIGN KEY (MateriaId) REFERENCES Materias(Id),
    CONSTRAINT FK_EC_Docentes  FOREIGN KEY (DocenteId) REFERENCES Docentes(Id),
    CONSTRAINT FK_EC_Cursos    FOREIGN KEY (CursoId)   REFERENCES Cursos(Id),
    CONSTRAINT UQ_EC UNIQUE (MateriaId, DocenteId, CursoId)
);

-- ──────────────────────────────────────────────────────────
-- INSCRIPCIONES
-- ──────────────────────────────────────────────────────────

CREATE TABLE InscripcionesMateria (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId     INT          NOT NULL,
    MateriaId        INT          NOT NULL,
    CursoId          INT          NOT NULL,
    Estado           NVARCHAR(20) NOT NULL DEFAULT 'Activa',
    FechaInscripcion DATETIME2    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_InscMat_Estudiante FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id),
    CONSTRAINT FK_InscMat_Materia    FOREIGN KEY (MateriaId)    REFERENCES Materias(Id),
    CONSTRAINT FK_InscMat_Curso      FOREIGN KEY (CursoId)      REFERENCES Cursos(Id),
    CONSTRAINT UQ_InscMat UNIQUE (EstudianteId, MateriaId, CursoId)
);

CREATE TABLE Examenes (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    MateriaId   INT          NOT NULL,
    FechaExamen DATE         NOT NULL,
    Horario     NVARCHAR(50) NOT NULL,
    Cupo        INT          NOT NULL,
    TipoExamen  NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Examenes_Materias FOREIGN KEY (MateriaId) REFERENCES Materias(Id),
    CONSTRAINT CK_Examenes_Cupo CHECK (Cupo > 0)
);

CREATE TABLE InscripcionesExamen (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId     INT           NOT NULL,
    ExamenId         INT           NOT NULL,
    Estado           NVARCHAR(20)  NOT NULL DEFAULT 'Activa',
    NotaValor        DECIMAL(4,2)  NULL,
    FechaInscripcion DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_InscEx_Estudiante FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id),
    CONSTRAINT FK_InscEx_Examen     FOREIGN KEY (ExamenId)     REFERENCES Examenes(Id),
    CONSTRAINT UQ_InscEx UNIQUE (EstudianteId, ExamenId),
    CONSTRAINT CK_InscEx_Nota CHECK (NotaValor IS NULL OR (NotaValor >= 1 AND NotaValor <= 10))
);

-- ──────────────────────────────────────────────────────────
-- ASISTENCIA
-- ──────────────────────────────────────────────────────────

CREATE TABLE Asistencias (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId INT          NOT NULL,
    MateriaId    INT          NOT NULL,
    CursoId      INT          NOT NULL,
    Fecha        DATE         NOT NULL,
    Estado       NVARCHAR(30) NOT NULL,
    Motivo       NVARCHAR(300) NULL,
    CONSTRAINT FK_Asist_Estudiante FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id),
    CONSTRAINT FK_Asist_Materia    FOREIGN KEY (MateriaId)    REFERENCES Materias(Id),
    CONSTRAINT FK_Asist_Curso      FOREIGN KEY (CursoId)      REFERENCES Cursos(Id),
    CONSTRAINT UQ_Asistencia UNIQUE (EstudianteId, MateriaId, CursoId, Fecha)
);

-- ──────────────────────────────────────────────────────────
-- HISTORIAL Y SEGUIMIENTO
-- ──────────────────────────────────────────────────────────

CREATE TABLE HistorialAcademico (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId INT           NOT NULL,
    MateriaId    INT           NOT NULL,
    CursoId      INT           NOT NULL,
    Anio         INT           NOT NULL,
    Comision     NVARCHAR(20)  NOT NULL,
    EstadoFinal  NVARCHAR(50)  NOT NULL,
    NotaFinal    DECIMAL(4,2)  NULL,
    Condicion    NVARCHAR(50)  NOT NULL,
    CONSTRAINT FK_Hist_Estudiante FOREIGN KEY (EstudianteId) REFERENCES Estudiantes(Id),
    CONSTRAINT FK_Hist_Materia    FOREIGN KEY (MateriaId)    REFERENCES Materias(Id),
    CONSTRAINT FK_Hist_Curso      FOREIGN KEY (CursoId)      REFERENCES Cursos(Id),
    CONSTRAINT CK_Hist_Nota CHECK (NotaFinal IS NULL OR (NotaFinal >= 1 AND NotaFinal <= 10))
);

CREATE TABLE Alertas (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    Anio                INT           NOT NULL,
    Comision            NVARCHAR(20)  NOT NULL,
    EstadoFinal         NVARCHAR(50)  NOT NULL,
    NotaFinal           DECIMAL(4,2)  NULL,
    Condicion           NVARCHAR(50)  NOT NULL,
    Enviada             BIT           NOT NULL DEFAULT 0,
    FechaCreacion       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    InscripcionExamenId INT           NULL,
    InscripcionMateriaId INT          NULL,
    ExamenId            INT           NULL,
    CONSTRAINT FK_Alert_InscEx  FOREIGN KEY (InscripcionExamenId)  REFERENCES InscripcionesExamen(Id),
    CONSTRAINT FK_Alert_InscMat FOREIGN KEY (InscripcionMateriaId) REFERENCES InscripcionesMateria(Id),
    CONSTRAINT FK_Alert_Examen  FOREIGN KEY (ExamenId)             REFERENCES Examenes(Id)
);

CREATE TABLE CalendarioAcademico (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    NombreEvento NVARCHAR(200) NOT NULL,
    Comision     NVARCHAR(20)  NULL,
    FechaInicio  DATE          NOT NULL,
    FechaFin     DATE          NOT NULL,
    TipoEvento   NVARCHAR(50)  NOT NULL,
    MateriaId    INT           NULL,
    CursoId      INT           NULL,
    CONSTRAINT FK_Cal_Materia FOREIGN KEY (MateriaId) REFERENCES Materias(Id),
    CONSTRAINT FK_Cal_Curso   FOREIGN KEY (CursoId)   REFERENCES Cursos(Id),
    CONSTRAINT CK_Cal_Fechas CHECK (FechaFin >= FechaInicio)
);

-- ──────────────────────────────────────────────────────────
-- ENCUESTAS (anónimas — sin FK al alumno)
-- ──────────────────────────────────────────────────────────

CREATE TABLE Encuestas (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    MateriaId     INT       NOT NULL,
    DocenteId     INT       NOT NULL,
    Preguntas     NVARCHAR(MAX) NOT NULL,
    Activa        BIT       NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Enc_Materia  FOREIGN KEY (MateriaId) REFERENCES Materias(Id),
    CONSTRAINT FK_Enc_Docente  FOREIGN KEY (DocenteId) REFERENCES Docentes(Id)
);

CREATE TABLE RespuestasEncuesta (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    EncuestaId  INT           NOT NULL,
    Preguntas   NVARCHAR(MAX) NOT NULL,
    Respuestas  NVARCHAR(MAX) NOT NULL,
    Fecha       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    -- Sin FK a alumno: anonimización CU-36/CU-40
    CONSTRAINT FK_Resp_Encuesta FOREIGN KEY (EncuestaId) REFERENCES Encuestas(Id)
);

-- ──────────────────────────────────────────────────────────
-- AUDITORÍA (CU-06): registro inmutable de cambios en Notas e Inscripciones
-- ──────────────────────────────────────────────────────────

CREATE TABLE AuditoriaCambios (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    TablaAfectada       NVARCHAR(100) NOT NULL,
    RegistroAfectado    NVARCHAR(50)  NOT NULL,
    Accion              NVARCHAR(50)  NOT NULL,
    FechaCambio         DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    ValorAnterior       NVARCHAR(MAX) NULL,
    ValorNuevo          NVARCHAR(MAX) NULL,
    UsuarioId           INT           NOT NULL,
    ExamenId            INT           NULL,
    CalendarioId        INT           NULL,
    InscripcionExamenId INT           NULL,
    InscripcionMateriaId INT          NULL,
    EncuestaId          INT           NULL,
    CONSTRAINT FK_Aud_Usuario    FOREIGN KEY (UsuarioId)            REFERENCES Usuarios(Id),
    CONSTRAINT FK_Aud_Examen     FOREIGN KEY (ExamenId)             REFERENCES Examenes(Id),
    CONSTRAINT FK_Aud_Calendario FOREIGN KEY (CalendarioId)         REFERENCES CalendarioAcademico(Id),
    CONSTRAINT FK_Aud_InscEx     FOREIGN KEY (InscripcionExamenId)  REFERENCES InscripcionesExamen(Id),
    CONSTRAINT FK_Aud_InscMat    FOREIGN KEY (InscripcionMateriaId) REFERENCES InscripcionesMateria(Id),
    CONSTRAINT FK_Aud_Encuesta   FOREIGN KEY (EncuestaId)           REFERENCES Encuestas(Id)
);

CREATE INDEX IX_AuditoriaCambios_Tabla ON AuditoriaCambios (TablaAfectada);
CREATE INDEX IX_AuditoriaCambios_Fecha ON AuditoriaCambios (FechaCambio);

-- ──────────────────────────────────────────────────────────
-- AUDITORÍA LEGACY (logs de acceso y cambios de rol)
-- ──────────────────────────────────────────────────────────

CREATE TABLE AuditoriaLogs (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    EntidadTipo   NVARCHAR(50)  NOT NULL,
    EntidadId     NVARCHAR(20)  NOT NULL,
    Accion        NVARCHAR(20)  NOT NULL,
    EjecutorEmail NVARCHAR(150) NOT NULL,
    EjecutorId    INT           NULL,
    ValorAnterior NVARCHAR(MAX) NULL,
    ValorNuevo    NVARCHAR(MAX) NULL,
    Timestamp     DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE AuditoriaCambiosRol (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId   INT          NOT NULL,
    RolOriginal NVARCHAR(50) NOT NULL,
    RolVista    NVARCHAR(50) NOT NULL,
    Accion      NVARCHAR(20) NOT NULL,
    Timestamp   DATETIME2    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AuditRol_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

CREATE TABLE LogsSeguridad (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Email       NVARCHAR(150) NOT NULL,
    Exitoso     BIT           NOT NULL,
    MotivoFallo NVARCHAR(200) NULL,
    IpOrigen    NVARCHAR(45)  NOT NULL,
    UserAgent   NVARCHAR(500) NOT NULL,
    Timestamp   DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
