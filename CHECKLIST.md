# Checklist de revisión completa — Sistema Académico Integral

Revisión meticulosa módulo por módulo, como si se recorriera la app a mano: lectura de código
`Controller → UseCase → Domain → Infrastructure` + frontend Angular, contrastada contra
[CLAUDE.md](CLAUDE.md) y [docs/casos-de-uso.md](docs/casos-de-uso.md), y ejercitada de verdad
con `curl` contra un backend real corriendo localmente. **Revisión completa. Sin fixes
aplicados todavía** — se decide qué reparar y en qué orden después de leer esto entero.

**Entorno de pruebas:** backend en `http://localhost:5050` (copia aislada del código, sin tocar
`bin/`/`obj/` del proyecto real ni el proceso que ya corría en el puerto 5000 contra Azure SQL),
contra SQL Server local en Docker (`checklist-sql-local`, puerto 14330) — **totalmente aislado
de la Azure SQL real**, sandbox descartable. 7 subagentes revisaron un grupo temático cada uno,
en paralelo, contra esta misma base.

---

## Resumen ejecutivo — hallazgos por severidad

### 🔴 Críticos (rompen funcionalidad core, posible impacto en producción)

1. ✅ **REPARADO — Autoinscripción de estudiantes a materias y exámenes completamente rota.**
   La migración `20260526000001_RedisenoEncuestas` nunca se aplicaba porque le faltaba el
   atributo `[Migration(...)]` (sin Designer.cs) — EF Core ni la veía como pendiente. Ver detalle
   completo de causa y fix en "Plan de reparación priorizado → Tier 1 #1". Verificado en Azure:
   sano, no había incidente activo en producción.
2. ✅ **REPARADO — Alta de Estudiante rota (500).** Falta de columna `FechaDeEgreso` en
   `Estudiantes`, mismo origen que el punto anterior (migración sin atributo `[Migration]`,
   invisible para EF). Ver "Plan de reparación priorizado → Tier 1 #1". Verificado en Azure:
   sano, no había incidente activo en producción.
3. ✅ **REPARADO — Race condition confirmada en cupo de curso.** Dos inscripciones concurrentes a
   un curso con `Cupo=1` quedaban ambas persistidas. Fix: transacción `Serializable` alrededor
   del conteo+insert. Ver "Plan de reparación priorizado → Tier 2". Verificado con 6 requests
   realmente concurrentes contra un cupo=1: exactamente 1 pasa, 5 rechazadas con 409.
4. ✅ **REPARADO — Sin transacción en alta de Estudiante, Registro autogestionado y Cierre de
   curso.** Los 3 (más el cupo de examen, ítem 3) ahora usan `IUnitOfWork.EjecutarEnTransaccionAsync`.
   Ver "Plan de reparación priorizado → Tier 2" para el detalle completo de causa y verificación.
5. ✅ **REPARADO — Dirección podía autodesactivarse sin ningún guard** (`CambiarActivacionUseCase`)
   — riesgo de lockout total del sistema si no quedaba ningún Dirección activo. Reproducido por
   el propio subagente, quien casi se bloqueó a sí mismo (recuperado a tiempo porque el JWT viejo
   seguía vigente — con el fix del Tier 3 #9 eso tampoco habría salvado la situación). Ver
   "Plan de reparación priorizado → Tier 3 #8".
6. ✅ **REPARADO — `DbUpdateException` no manejada en el middleware global** (`Program.cs:444-453`).
   Causa raíz compartida por al menos 4 hallazgos distintos (correlatividad duplicada, eliminar
   materia con cátedra, doble-inscripción concurrente, colisión de código de materia): cualquier
   violación de constraint de BD que debería ser un 409/400 amigable caía en un 500 genérico.
   Ver "Plan de reparación priorizado → Tier 1 #2". Nota: esto arregla el *código de respuesta*
   de esos 4 síntomas; la causa de fondo de la race condition del punto #3 se resolvió aparte
   con el fix de transacciones del Tier 2.
7. ✅ **REPARADO — `POST /api/examenes/{id}/inscripciones` con rol Dirección era código muerto.**
   El endpoint declaraba `Roles = "Estudiante,Direccion"` pero el UseCase siempre resolvía el
   estudiante desde el token del que llama, sin parámetro para indicar a quién inscribir.
   Ver "Plan de reparación priorizado → Tier 4 #12".

### 🟠 Altos

- ✅ REPARADO — JWT no se revocaba al desactivar una cuenta (seguía válido hasta 8h después). Ver
  Tier 3 #9.
- ✅ REPARADO — Sin validación de formato de DNI/Email en altas. Ver Tier 5 #16.
- ✅ REPARADO — RBAC de `EstudiantesController` vs. `docs/casos-de-uso.md`. Ver Tier 5 #17
  (se corrigió la documentación, el código de Direccion-only era el correcto).
- ✅ REPARADO — `POST /api/auditoria/cambio-rol` era Direccion-only pero el frontend deja que un
  Docente dispare esa llamada. Ver Tier 5 #18.
- ✅ REPARADO — Sin rate limiting / lockout por fuerza bruta en `/api/auth/login`. Ver Tier 3 #10.
- ✅ REPARADO — `/api/auth/olvide-password` filtraba mensajes de excepción internos (p. ej. de
  Azure SDK) al cliente. Ver Tier 3 #11.
- ⏸️ No aplica (decisión del usuario) — `CarrerasController` no tiene CRUD real. Ver Tier 6 #26.
- ✅ REPARADO — `EspacioCurricular` no validaba que Materia y Curso pertenecieran a la misma
  Carrera. Ver Tier 5 #23.
- ✅ REPARADO — Se podía crear una cátedra (`EspacioCurricular`) sobre un Curso ya `Cerrado`. Ver
  Tier 5 #24.
- ✅ REPARADO — Correlatividades no generaban registro de auditoría. Ver Tier 5 #25.
- ✅ REPARADO — CU-47 (validación temporal de inscripción) era global. Ver Tier 5 #19.
- ✅ REPARADO — "Cierre de actas" solo existía para Dirección, no para Preceptor. Ver Tier 5 #20.
- ✅ REPARADO — `mis-examenes.component.ts` no tenía el fix fail-open de encuesta que sí tiene
  `mis-materias.component.ts` (commit `ffe69ff`). Ver Tier 7 #30.
- ✅ REPARADO — Dar de baja de inscripción a materia era Direccion-only, contradiciendo
  `docs/casos-de-uso.md`. Ver Tier 5 #21.
- ✅ REPARADO — `ModificarEstudianteUseCase` permitía forzar `Egresado`/`Desertor` manualmente sin
  validar plan completo ni los 2 años de inactividad. Ver Tier 5 #22.
- ✅ REPARADO — Alertas de "plazo de carga de notas" nunca llegaban al docente destinatario. Ver
  Tier 6 #27.
- ✅ REPARADO — `Curso.Crear` con los campos `Anio`/`AnioLectivo` semánticamente invertidos. Ver
  Tier 7 #31.
- ✅ REPARADO — `CrearCursoDto.PreceptorId` en realidad esperaba el `UsuarioId` del preceptor. Ver
  Tier 7 #32.
- ✅ REPARADO — Regla anti-PII del Asistente IA sin ningún test/control automático. Ver Tier 6 #29.
- ⏸️ Pendiente (decisión del usuario) — Certificados con Hash SHA-256/QR no implementados. Ver
  Tier 6 #28.
- ✅ REPARADO — Backend aceptaba contraseñas de 6 caracteres, frontend exige 8+. Ver Tier 7 #33.

### 🟡 Medios / cosméticos

- ✅ REPARADO — `CarrerasService.carreras$` era `readonly` en el frontend. Ver Tier 7 #34.
- ✅ REPARADO — Varios controllers documentaban `404 NotFound` en Swagger pero el código real
  devuelve `400`. Ver Tier 7 #35.
- `CerrarCursoUseCase` no valida idempotencia — **corregido como parte del Tier 2 #7**
  (`Curso.Cerrar()` ahora rechaza cerrar un curso ya cerrado).
- ✅ REPARADO — `InscripcionMateriaResultDto.materiaNombre` viajaba vacío. Ver Tier 7 #36.
- ✅ REPARADO — `RestablecerPasswordUseCase` no validaba longitud mínima de la nueva contraseña.
  Ver Tier 7 #37.
- ✅ REPARADO — Sin capacidad de cierre remoto de sesión. Ver Tier 7 #38.
- ✅ REPARADO — Mensaje de error confuso ("El usuario no es un usuario") al usar el endpoint de
  desactivación equivocado. Ver Tier 7 #39.

---

## Setup del entorno

- [x] Contenedor SQL Server local levantado y accesible (`checklist-sql-local`, puerto 14330)
- [x] Backend corriendo contra la BD local (puerto 5050, copia aislada en scratchpad)
- [x] Migraciones aplicadas (25 tablas), admin inicial generado — ⚠️ con los gaps de migración
  documentados en los hallazgos críticos #1 y #2
- [ ] Frontend corriendo (`ng serve`) — no se llegó a levantar, toda la revisión de frontend fue
  por lectura de código
- [x] Usuarios de prueba por rol creados y usados por los 7 subagentes en paralelo

---

## Grupo 1 — Identidad & Acceso

- [x] Login con credenciales válidas (los 4 roles) — 200, JWT bien formado.
- [x] Login con credenciales inválidas — password incorrecta y usuario inexistente devuelven
  ambos `401` con mensaje idéntico (no filtra existencia de cuenta).
- [x] Login con usuario inactivo — `401 "La cuenta está desactivada."`, ciclo completo
  desactivar→login falla→reactivar→login OK verificado.
- [!] Estructura/expiración del JWT — HS256, `exp`=8h. El JWT sigue siendo válido tras
  desactivar la cuenta (ver Hallazgo).
- [x] Unicidad DNI/Legajo/Email — probada cruzando Docente/Preceptor/Estudiante (comparten
  `Usuarios`), incluye normalización de email a lowercase.
- [!] Validación de formato en alta (DNI/email) — no existe a nivel backend, solo unicidad.
- [x] RBAC cruzado — matriz completa con los 4 tokens contra todos los endpoints del grupo,
  internamente consistente.
- [!] RBAC vs. documentación — `casos-de-uso.md` dice que Preceptor puede gestionar Estudiantes;
  el código lo restringe a Direccion exclusivamente.
- [x] Baja/Reactivación — ciclo completo verificado, `CambiarActivacionUseCase` valida
  `rolEsperado` correctamente.
- [!] Autodesactivación / lockout — Dirección pudo autodesactivarse sin guard (ver Hallazgo,
  **ya restaurado**).
- [x] AuditoriaCambioRol (vista de rol, no cambio de rol real) — se registra correctamente, el
  rol efectivo de acceso usa el rol real del JWT, no el `rolVista`.
- [!] AuditoriaCambioRol RBAC vs. frontend — endpoint Direccion-only, frontend deja intentarlo a
  Docente (falla silenciosa).
- [x] Padrón — validación de formato robusta (solo dígitos, 7-10 caracteres), duplicados → 409,
  import batch con errores por fila sin abortar, RBAC Direccion-only.
- [x] Registro autogestionado — exige DNI habilitado en padrón, siempre crea rol Estudiante, no
  revela si el DNI ya tiene usuario. Sin transacción entre los 3 pasos (Usuario/Estudiante/Padrón).
- [x] Sesiones — heartbeat y listado de activas funcionan, implementación en memoria aceptable
  para el alcance actual.
- [ ] Cierre remoto de sesión — no existe esa función.
- [x] LogsSeguridad — login fallido y exitoso generan fila correctamente, RBAC Direccion-only.
- [!] Sin rate limiting / lockout por fuerza bruta en login.
- [x] Frontend guards vs RBAC backend — consistentes entre sí (heredan el mismo desvío vs.
  `casos-de-uso.md`).
- [x] Frontend validaciones de formulario — más estrictas que el backend (bypasseable por API
  directa).
- [x] Reset de contraseña — no filtra existencia de email, token expira en 1h, valida coincidencia.
- [!] Envío de email de reset — filtra excepción interna del SDK de Azure al cliente si falla el
  envío; token se genera igual.

### Hallazgos — ver Resumen ejecutivo (Críticos #4, #5; Altos: JWT no revocado, sin validación
de formato, RBAC vs. docs, AuditoriaCambioRol frontend/backend, sin rate limiting, reset-password
filtra detalles, password débil aceptada; Medios: sin cierre remoto de sesión, RestablecerPassword
sin validar fortaleza)

### Notas
- `SesionService` es singleton en memoria — no escala a múltiples instancias sin store compartido
  (aceptable para el alcance actual).
- JWT en `localStorage`, no cookie `httpOnly` — patrón común en SPA, expone el token a XSS si
  alguna vez se introduce esa vulnerabilidad.
- Estado de la BD sandbox restaurado tras las pruebas: `admin@institucion.edu.ar` y
  `docente.qa1@institucion.edu.ar` quedaron `Activo=true`.

---

## Grupo 2 — Estructura Académica

- [!] CRUD de Carreras — no existe (solo `GET`, confirmado `POST→405`, `PUT/DELETE→404`, sin UI).
- [x] `CarrerasService` (frontend) usa `shareReplay(1)` correctamente — sin bug activo hoy
  porque no hay mutaciones, pero ver hallazgo del campo `readonly`.
- [x] Materias — código autogenerado único, `CarreraId`/`Anio` validados, RBAC correcto.
- [x] Motor de correlatividades — autorreferencia y ciclos (DFS) rechazados correctamente, tipo
  de requerimiento y condición académica usados aguas abajo. RBAC correcto.
- [!] Correlatividades — duplicado exacto no validado a nivel de aplicación, cae en 500 (ver
  causa raíz común en Resumen ejecutivo).
- [x] EspaciosCurriculares — unicidad con chequeo explícito + índice BD, mensaje amigable, RBAC
  correcto.
- [!] EspaciosCurriculares — no valida que Materia y Curso sean de la misma Carrera.
- [x] Cursos — unicidad `(Anio, AnioLectivo, Comision, CarreraId)` con doble protección, RBAC
  correcto, cerrar/reactivar persisten estado.
- [!] Cursos — se puede crear una cátedra sobre un Curso ya `Cerrado`, sin validación ni
  documentación de que sea intencional.
- [!] Eliminar Materia no chequea `EspacioCurricular` asociado — 500 por violación de FK.
- [x] RBAC general del grupo — coherente y sin fugas en todos los endpoints.

### Hallazgos — ver Resumen ejecutivo (Críticos: correlatividad duplicada→500, eliminar materia
con cátedra→500; Altos: sin CRUD de Carreras, EspacioCurricular sin validar misma Carrera,
cátedra sobre curso cerrado, sin auditoría en Correlatividades; Medios: `carreras$` readonly,
404 documentado vs 400 real)

Adicional no listado arriba: **race condition en generación de código de Materia** (`MAT-XXX`,
`MateriaRepository.cs:36`, `MAX+1` sin lock) — bajo 5 requests concurrentes, 2 de 5 fallaron con
500 por colisión de código único.

---

## Grupo 3 — Inscripciones & Calendario

- [x] Inscripción a materia: validación automática de correlatividades — funciona en el camino feliz.
- [!] Doble inscripción — rechazada en secuencial; bajo concurrencia real cae en 500 sin manejar.
- [!] Cupos — race condition real y confirmada (ver Resumen ejecutivo, Crítico #3).
- [!] CU-47 (validación temporal) — el motor existe pero ignora comisión/materia/curso (ver
  Resumen ejecutivo).
- [ ] Inscripción a examen (Regularizado vs Aprobado) — confirmado por código, no se pudo
  ejercitar en vivo por el bug de encuestas (Crítico #1).
- [!] Cierre de actas por Preceptores — no existe ese endpoint, solo Dirección puede cerrar cursos.
- [x]/[!] Encuesta obligatoria — el gate server-side es robusto en diseño (no bypasseable desde
  el frontend), pero el subsistema está caído y bloquea toda la funcionalidad (Crítico #1).

### Hallazgos — ver Resumen ejecutivo (Críticos #1, #3, #7; Altos: CU-47 global, cierre de actas
sin Preceptor, mis-examenes.component.ts sin fix fail-open, baja de inscripción Direccion-only
contradice docs; Medios: CerrarCursoUseCase sin idempotencia, materiaNombre vacío)

Adicional: la generación de código de Materia (mismo bug que Grupo 2) produjo un 500 real
durante el setup de datos por la carga concurrente de otros grupos trabajando en paralelo sobre
la misma BD — confirma que la race condition es reproducible bajo uso normal, no solo con
scripts de ataque deliberados.

---

## Grupo 4 — Calificaciones & Historial

- [x] Invariante Nota (rango 1-10) — rechazada vía API en los 3 casos de borde probados (0, 11, -1).
- [x] Aprobado ≥ 4 — verificado con nota=4 (Aprobada) y nota=3.99/3 (Desaprobada).
- [x] Redondeo a 2 decimales — verificado.
- [x] Integridad referencial — inscripción inexistente → 400 controlado, no crashea.
- [x] Rectificación sólo desde Aprobada/Desaprobada — enforced en el dominio, no solo el UseCase.
- [x] AuditoriaCambio inmutable — cada carga/rectificación genera fila; no existe ningún
  endpoint PUT/DELETE sobre auditoría en todo el backend.
- [x] Máquina de estados — transición inválida (Egresado→Regular) rechazada por el dominio.
- [x] Evento MateriaAprobada → Egreso automático — confirmado como código real ejecutándose en
  el flujo de carga de nota (no aspiracional), no se pudo probar E2E completo por tamaño del
  plan sembrado.
- [x] RBAC carga/rectificación de notas — solo Docente, ni Dirección puede; aislamiento
  adicional por materia (el Docente debe ser titular de esa cátedra).
- [x] RBAC estado académico — Direccion+Preceptor, Docente excluido correctamente.
- [x] Tests unitarios de dominio — 53/53 passed (Nota, Estudiante, InscripcionExamen).

### Hallazgos — ver Resumen ejecutivo (Alto: egreso manual sin validar plan completo; Medio: 404
documentado vs 400 real; Crítico #1 reconfirmado desde este ángulo — se tuvo que insertar el
`InscripcionExamen` de prueba directo por SQL para poder testear Calificaciones)

---

## Grupo 5 — Asistencias & Alertas

- [x] Registro de asistencia diaria — duplicado sobre misma combinación rechazado con 400 limpio.
- [x] RBAC de carga — solo Docente.
- [x] RBAC de lectura/rectificación — Docente+Preceptor.
- [x] Rectificación — exige motivo cuando corresponde, genera auditoría inmutable.
- [x] Disparo manual de Alertas (`/detectar-riesgo`, `/vencimientos`) — permiten probar sin
  esperar al job semanal (lunes 08:00).
- [x] Notificación in-app junto a cada Alerta — confirmado en código y runtime, mismo método,
  mismo `SaveChanges`.
- [x] Deduplicación diaria de alertas — funciona.
- [x] `GET /api/notificaciones` — cada usuario ve solo las suyas.
- [x] IDOR en marcar notificación como leída — rechazado con 403 explícito al intentar cross-usuario.
- [!] Alertas de plazo de carga de notas a Docentes — la rama de código existe pero es
  inalcanzable (ver Resumen ejecutivo, Alto).
- [ ] Envío de email real (ACS) — no verificable en local sin `AzureCommunication:ConnectionString`,
  esperado, no bloquea el resto del flujo.

### Hallazgos — ver Resumen ejecutivo (Alto: alertas de vencimiento nunca llegan al docente,
`Curso.Crear` con campos invertidos, `PreceptorId` en realidad `UsuarioId`)

---

## Grupo 6 — Encuestas

- [!] Módulo funcional en general — **completamente caído** en este entorno (ver Resumen
  ejecutivo, Crítico #1), bloqueante para probar el resto del grupo en runtime.
- [x] Anonimización (diseño de código) — sin FK a Estudiante, disociación vía
  HMAC-SHA256(estudianteId|encuestaId, key=salt secreto obligatorio al arrancar). Diseño correcto.
- [!] Anonimización (esquema real en BD) — no verificable, las tablas nuevas no existen.
- [x] Aplicación automática al inscribirse — gate server-side confirmado en código (no solo
  frontend), pero no ejercitable en runtime por el Crítico #1.
- [ ] Completar encuesta / EncuestaCompletada — no probado E2E, bloqueado por el mismo motivo.
- [x] RBAC — probado y correcto en todos los endpoints (corre antes de tocar las tablas rotas).

### Hallazgos — ver Resumen ejecutivo, Crítico #1 (causa raíz completa documentada acá: migración
`20260526000001_RedisenoEncuestas` nunca aplicada, columnas viejas siguen en `Encuestas`/
`RespuestasEncuesta`, tablas nuevas `PreguntasEncuesta`/`ItemsRespuesta`/`EncuestasCompletadas`
no existen — **pendiente confirmar si Azure real tiene el mismo problema**)

Diseño de seguridad adicional confirmado: `Program.cs` exige `Encuestas:Salt` obligatorio al
arrancar (falla el startup si falta), evitando un default público predecible; `appsettings.json`
versionado en git solo tiene el placeholder, no un secreto real.

---

## Grupo 7 — Reportes & Asistente IA

- [x] RBAC ReportesCohorteController — Direccion-only, verificado en 6+ endpoints y sus /pdf.
- [x] RBAC ReportesRendimientoController — Direccion+Docente (Docente ve solo sus cátedras).
- [x] RBAC ReportesOperativosController — Preceptor+Direccion+Docente (RR-08), Preceptor+Direccion
  (RR-09), coincide con CLAUDE.md.
- [x] RBAC AsistenteIAController — Direccion-only server-side, no depende del frontend.
- [x] Dashboards con datos reales — tasas 0-100%, sin negativos ni NaN, contra datos sembrados
  por los otros grupos.
- [x] Generación de PDF — verificado (PDF válido, Content-Type correcto).
- [x] Anti-PII asistente IA — cumplida hoy en las 2 herramientas que la necesitan (ver hallazgo
  sobre falta de safety net automático).
- [x] Frontend — botón flotante gateado por `rolVista`, pero el guard de rutas usa el rol real
  del JWT, no la vista — consistente con el backend.
- [!] Certificados con Hash SHA-256/QR — no encontrados en el código, requerimiento documentado
  sin construir.

### Hallazgos — ver Resumen ejecutivo (Altos: anti-PII sin test/safety-net, certificados no
implementados)

---

## Verificación contra Azure SQL real (solo lectura, autorizada por el usuario)

Se confirmó por consulta directa (`SELECT` únicamente, sin escribir nada) que **la base de
producción está sana**: existen la columna `FechaDeEgreso` en `Estudiantes` y las tablas
`PreguntasEncuesta`/`ItemsRespuesta`/`EncuestasCompletadas`. Los dos hallazgos críticos de
migración (#1 y #2) son específicos de haber migrado una base **nueva y vacía** desde cero — en
Azure esas migraciones ya corrieron bien hace tiempo sobre una base que fue creciendo de forma
incremental, así que el mecanismo de "reparación" nunca se disparó ahí. **No hay incidente
activo en producción.**

El resto de los hallazgos (bugs de lógica de código: transacciones faltantes, race conditions,
RBAC, validaciones) no se verificaron contra Azure porque reproducirlos ahí implicaría pruebas
mutantes/destructivas sobre datos reales. Al ser lógica de código (no estado de datos), se
asumen aplicables a producción si el código desplegado coincide con este repo — se corrigen en
base al análisis ya hecho, no hace falta reproducirlos en vivo contra la base real.

---

## Plan de reparación priorizado

Bugs y hallazgos encontrados, en el orden recomendado para repararlos. El orden prioriza: (1)
causas raíz que explican varios síntomas a la vez, (2) impacto en flujos centrales del sistema,
(3) riesgo/costo de la corrección. Ninguno de estos fixes se aplicó todavía.

### Tier 1 — Causas raíz sistémicas (arrancar por acá: desbloquean o simplifican todo lo demás)

1. ✅ **REPARADO — `RepararMigracionesInconsistentesAsync` no lograba que `Migrate()` reaplicara
   DDL, y además revienta sin control en una BD vacía.**

   **Causa real (más profunda de lo que se sospechaba inicialmente):** no era un problema del
   mecanismo de reparación en sí. Las tres migraciones involucradas —
   `20260526000001_RedisenoEncuestas`, `20260529000001_AddFechaDeEgresoEstudiante` y
   `20260529000002_EnsureFechaDeEgresoEstudiante` — **no tenían archivo `.Designer.cs`, y por lo
   tanto ninguna tenía el atributo `[Migration("id")]`**. Sin ese atributo, el mecanismo de
   descubrimiento de migraciones de EF Core (`MigrationsAssembly`, que escanea tipos con
   `[Migration(...)]`) directamente no las registra como existentes — no es que fallen, son
   invisibles para `Migrate()`. Confirmado comparando `__EFMigrationsHistory` de una BD recién
   migrada desde cero (solo 14 migraciones aplicadas, saltando exactamente esas 3) contra la
   lista real de archivos en `backend/src/Migrations/`. El `AppDbContextModelSnapshot.cs` ya
   reflejaba correctamente `FechaDeEgreso`/`Titulo`/`CicloLectivo`, confirmando que el modelo
   C# siempre estuvo "adelantado" — solo faltaba que las migraciones se ejecutaran.

   **Fix aplicado:**
   - Se agregó `[DbContext(typeof(AppDbContext))]` + `[Migration("<id>")]` directamente a las 3
     migraciones huérfanas ([20260526000001_RedisenoEncuestas.cs](backend/src/Migrations/20260526000001_RedisenoEncuestas.cs),
     [20260529000001_AddFechaDeEgresoEstudiante.cs](backend/src/Migrations/20260529000001_AddFechaDeEgresoEstudiante.cs),
     [20260529000002_EnsureFechaDeEgresoEstudiante.cs](backend/src/Migrations/20260529000002_EnsureFechaDeEgresoEstudiante.cs)),
     haciéndolas visibles para `Migrate()`.
   - Se reescribió `RepararMigracionesInconsistentesAsync` ([Program.cs:479-528](backend/src/Program.cs#L479-L528))
     para que deje de borrar filas de `__EFMigrationsHistory` (mecanismo que se demostró poco
     confiable: `Migrate()` podía reportar "already up to date" sin reaplicar el DDL) y en su
     lugar aplique el DDL faltante de forma directa e idempotente, sin tocar la tabla de
     historial. Se salta automáticamente si la tabla base todavía no existe, lo que además
     elimina el crash original en una base de datos vacía. Queda como red de seguridad
     adicional, ya no como mecanismo principal.
   - **Verificado seguro para Azure real** (solo lectura): `RedisenoEncuestas` ya está en el
     historial de Azure (se saltea, tablas ya existen — sin riesgo de recrearlas). Las otras dos
     no están en el historial de Azure pero son idempotentes (`IF NOT EXISTS`), así que
     aplicarlas ahí sería un no-op seguro que solo registra el ID en el historial.
   - **Verificación end-to-end:** BD local recreada completamente desde cero → arranca sin
     crashear, las 3 migraciones se aplican en orden (`Applying migration '20260526000001...'`,
     `'20260529000001...'`, `'20260529000002...'`), columna `FechaDeEgreso` y tablas
     `PreguntasEncuesta`/`ItemsRespuesta`/`EncuestasCompletadas` confirmadas presentes por SQL
     directo, `POST /api/estudiantes` → `201 Created` (antes: 500), `GET /api/encuestas` →
     `200 []` (antes: 500). Suite de tests unitarios: **142/142 passed**, sin regresiones.

2. ✅ **REPARADO — `DbUpdateException` no manejada en el middleware global de excepciones.**

   **Causa:** [Program.cs:436-470 (antes del fix)](backend/src/Program.cs#L436-L470) — el
   `switch` de mapeo de excepciones no tenía ningún caso para `Microsoft.EntityFrameworkCore.DbUpdateException`
   (la excepción que EF Core lanza ante cualquier violación de constraint de BD: índice único,
   FK, etc.), así que caía siempre en el `_ => 500 "Error interno del servidor"` genérico —
   causa raíz compartida de al menos 4 síntomas distintos ya documentados (correlatividad
   duplicada, eliminar materia con cátedra asignada, doble-inscripción concurrente, colisión de
   código de materia).

   **Fix aplicado:** se agregó un caso `DbUpdateException => (409, "Conflicto de datos")` al
   switch, con un mensaje de detalle genérico y fijo ("El cambio no se pudo guardar por un
   conflicto con los datos existentes...") en vez de `ex.Message` — evita el riesgo de que el
   texto de `DbUpdateException`/su `InnerException` filtre nombres de tabla o detalles de SQL
   Server al cliente.

   **Verificación end-to-end:** crear una correlatividad duplicada (mismo destino+requisito+tipo)
   → antes `500`, ahora `409 {"title":"Conflicto de datos", "detail":"El cambio no se pudo
   guardar por un conflicto con los datos existentes..."}`. Suite de tests: sin regresiones
   (incluida en el 142/142 de arriba, ya que el middleware no tiene tests propios pero no rompió
   ningún test de dominio existente).

### Tier 2 — Falta de transacciones / integridad de datos (mismo defecto de diseño, 5 lugares) ✅ REPARADO

**Causa común a los 5 puntos:** cada UseCase hacía 2-3 llamadas a distintos repositorios, y cada
`AgregarAsync`/`GuardarCambiosAsync` ejecuta su propio `SaveChangesAsync` de forma independiente
(confirmado leyendo `UsuarioRepository`, `EstudianteRepository`, `InscripcionMateriaRepository`,
etc. — todas comparten la misma instancia de `AppDbContext` inyectada por request, pero cada
método persiste por su cuenta). Si un paso posterior fallaba, los pasos anteriores ya habían
quedado confirmados en la base — sin forma de revertirlos.

**Fix aplicado (uno solo, reutilizado en los 5 lugares):** se agregó una interfaz
[`IUnitOfWork`](backend/src/Application/Interfaces/IUnitOfWork.cs) (en `Application/Interfaces`,
mismo lugar que el resto de las interfaces de repositorio) con un método
`EjecutarEnTransaccionAsync(Func<Task> operacion, IsolationLevel isolationLevel, CancellationToken)`,
implementado en [`UnitOfWork.cs`](backend/src/Infrastructure/Persistence/UnitOfWork.cs)
envolviendo `AppDbContext.Database.BeginTransactionAsync(...)` dentro de
`CreateExecutionStrategy().ExecuteAsync(...)` (obligatorio porque el `DbContext` usa
`EnableRetryOnFailure`, que no permite transacciones manuales sin ese envoltorio). Registrado en
DI (`Program.cs`) como `Scoped`, igual que los repositorios.

Nota técnica: hubo que invocar `RelationalDatabaseFacadeExtensions.BeginTransactionAsync(...)` y
`ExecutionStrategyExtensions.ExecuteAsync(...)` de forma completamente calificada (no como
`context.Database.BeginTransactionAsync(...)` normal) porque `DatabaseFacade`/`IExecutionStrategy`
tienen miembros de instancia con el mismo nombre que bloquean la resolución de esas sobrecargas
de extensión en C# (un gotcha conocido del lenguaje: la sola presencia de un método de instancia
con el mismo nombre descarta todas las extensiones, aunque ninguno matchee los argumentos).

3. ✅ **`CrearEstudianteUseCase` sin transacción** —
   [CrearEstudianteUseCase.cs:33-45](backend/src/Application/Estudiantes/CrearEstudianteUseCase.cs#L33-L45).
   Alta de `Usuario` + `Estudiante` + auditoría envueltas en `EjecutarEnTransaccionAsync`
   (aislamiento por defecto, `ReadCommitted` — acá solo hacía falta atomicidad, no proteger un
   conteo). **Verificado:** alta normal sigue devolviendo `201`; alta con DNI duplicado sigue
   devolviendo `409` limpio (falla antes de la transacción, ni siquiera la abre).
4. ✅ **`RegistroUseCase` (autogestión) sin transacción** —
   [RegistroUseCase.cs:32-42](backend/src/Application/Auth/RegistroUseCase.cs#L32-L42). Usuario +
   Estudiante + consumo del Padrón envueltos igual. **Verificado:** registro autogestionado
   end-to-end (carga de padrón → `POST /api/auth/registro`) → `201`.
5. ✅ **Race condition en cupo de curso** —
   [InscribirseEnMateriaUseCase.cs:58-75](backend/src/Application/Inscripciones/InscribirseEnMateriaUseCase.cs#L58-L75).
   El conteo de "activas en curso" + el insert ahora corren dentro de
   `EjecutarEnTransaccionAsync(..., IsolationLevel.Serializable, ...)` — a diferencia de
   `ReadCommitted`, `Serializable` sí evita que dos transacciones concurrentes lean el mismo
   conteo antes de que ninguna haga commit (previene phantom reads sobre el rango contado).
   **Verificado con concurrencia real:** curso con cupo=1, 6 inscripciones disparadas en paralelo
   de verdad (`curl ... & ... wait`) → exactamente **1 de 6** con `201`, las otras 5 con `409
   "No hay cupo disponible en este curso."` (antes: 2 pasaban el cupo). Confirmado además por
   SQL directo: 1 sola fila `Activa` para ese curso+materia.
6. ✅ **Mismo patrón en cupo de examen** —
   [InscribirseEnExamenUseCase.cs:51-84](backend/src/Application/Inscripciones/InscribirseEnExamenUseCase.cs#L51-L84).
   Mismo fix (`Serializable`), aplicado también a la validación de correlatividades para rendir
   (queda dentro de la misma transacción, sin efecto adverso porque es de solo lectura).
7. ✅ **`CerrarCursoUseCase` sin transacción envolvente, y sin validar idempotencia** —
   [CerrarCursoUseCase.cs:25-66](backend/src/Application/Cursos/CerrarCursoUseCase.cs#L25-L66).
   Cierre del curso + liquidación de cada inscripción + alta de historial ahora en una sola
   transacción. Además se agregó el guard de idempotencia directamente en el dominio —
   [`Curso.Cerrar()`](backend/src/Domain/Entities/Curso.cs#L39-L44) ahora lanza
   `BusinessException("El curso ya está cerrado.", 409)` si el curso ya estaba `Cerrado`, en vez
   de aceptarlo silenciosamente. **Verificado:** primer cierre → `204`; segundo cierre del mismo
   curso → `409` (antes: `204` silencioso).

**Regresión:** 2 tests unitarios existentes (`InscribirseEnExamenUseCaseTests.cs`,
`CerrarCursoUseCaseTests.cs`) instanciaban estos UseCases directamente y necesitaron un
`IUnitOfWork` de prueba — se agregó [`NoOpUnitOfWork`](backend/tests/TestSupport/NoOpUnitOfWork.cs)
en `TestSupport` (mismo patrón que el `NoOpAuditoriaService` ya existente: ejecuta la operación
directo, sin transacción real, porque el proveedor InMemory que usan estos tests no soporta
transacciones reales y la atomicidad en sí es una preocupación de infraestructura ajena a lo que
esos tests verifican). **Suite completa: 142/142 passed** tras el fix.

### Tier 3 — Seguridad y control de acceso ✅ REPARADO

8. ✅ **Dirección podía autodesactivarse sin ningún guard** —
   [CambiarActivacionUseCase.cs](backend/src/Application/Usuarios/CambiarActivacionUseCase.cs).
   **Fix:** antes de desactivar, si el usuario objetivo es rol `Direccion`, se cuentan otros
   usuarios `Direccion` activos (excluyendo al que se está por desactivar); si el resultado es
   0, se rechaza con `409 "No se puede desactivar: es el único usuario de Dirección activo..."`.
   Deliberadamente más general que "no autodesactivarse": también bloquea que Dirección A
   desactive a Dirección B si B fuera la última cuenta activa de ese rol — el riesgo real es
   quedarse sin ningún Dirección activo, no quién ejecuta la acción. **Verificado:**
   `DELETE /api/usuarios/1` (única cuenta Direccion) con su propio token → `409` (antes: `204` y
   la cuenta quedaba bloqueada).
9. ✅ **JWT no se revocaba al desactivar una cuenta** — [Program.cs:253-285](backend/src/Program.cs#L253-L285).
   **Fix:** se agregó `JwtBearerEvents.OnTokenValidated`, que en cada request autenticado hace una
   consulta liviana (por Id, indexada) para confirmar que el usuario del token sigue `Activo`; si
   no, `context.Fail(...)` y la request se rechaza con 401 en el acto, sin esperar a que expire
   el token (hasta 8h antes). **Verificado:** token de un docente funciona (`200`) antes de
   desactivarlo; el Direccion lo desactiva; el **mismo token viejo**, sin volver a loguearse,
   pasa a devolver `401` inmediatamente.
10. ✅ **Sin rate limiting / lockout por fuerza bruta en `/api/auth/login`** —
    [AuthController.cs](backend/src/Controllers/AuthController.cs). **Fix:** nueva policy `"login"`
    en `Program.cs` (`AddRateLimiter`), fixed window de 8 intentos por 5 minutos particionado por
    IP (no hay otra identidad disponible antes de autenticar), sin cola — el que excede el
    límite se rechaza al instante con `429` y un body JSON explicativo (antes: sin body,
    respuesta por defecto de ASP.NET). Aplicada con `[EnableRateLimiting("login")]` solo sobre el
    endpoint de login. **Verificado:** 10 intentos fallidos seguidos → los primeros pasan (401
    normal), a partir del 9° (contando los logins previos de otras pruebas desde la misma IP en
    la misma ventana) → `429`, matemática exacta con el límite configurado.
11. ✅ **`/api/auth/olvide-password` filtraba detalles internos de excepciones y generaba el token
    de reset aunque el envío de email fallara** —
    [SolicitarRestablecimientoUseCase.cs](backend/src/Application/Auth/SolicitarRestablecimientoUseCase.cs).
    **Fix:** se envolvió la llamada a `emailService.EnviarResetPasswordAsync` en un `try/catch`
    que loguea la excepción completa server-side (`ILogger`) y no la re-lanza — el endpoint
    siempre responde con el mensaje genérico de siempre, sin importar si el email existe, está
    activo, o si el proveedor de correo falló. **Verificado:** `POST /api/auth/olvide-password`
    con `AzureCommunication:ConnectionString` vacío (como en este entorno local) → `200` con el
    mensaje genérico de siempre (antes: `400` con `"Value cannot be an empty string. (Parameter
    'connectionString')"` expuesto al cliente); el log del servidor sí registra el error completo
    para diagnóstico interno.

**Regresión:** suite completa **142/142 passed** sin cambios adicionales en tests (estos 4 fixes
no tocaron ninguna lógica de dominio pura cubierta por los tests unitarios existentes).

### Tier 4 — Endpoints rotos o muertos ✅ REPARADO

12. ✅ **`POST /api/examenes/{id}/inscripciones` con rol Dirección era código muerto** —
    [ExamenesController.cs](backend/src/Controllers/ExamenesController.cs). El UseCase siempre
    resolvía el estudiante desde el token de quien llama, sin parámetro para indicar a quién
    inscribir. **Fix:** mismo patrón ya usado para materias
    (`InscribirseEnMateriaUseCase`/`InscribirseEnMateriaAutogestUseCase`) — se separó en dos:
    [`InscribirseEnExamenUseCase`](backend/src/Application/Inscripciones/InscribirseEnExamenUseCase.cs)
    ahora recibe `EstudianteId` explícito (sin el chequeo de encuesta, que es específico del
    flujo autogestionado) y queda expuesto en un endpoint nuevo, `POST /api/inscripciones/examenes`
    (Direccion-only, en `InscripcionesController`, junto al equivalente de materias). El endpoint
    viejo `POST /api/examenes/{examenId}/inscripciones` pasa a ser Estudiante-only y delega a la
    nueva [`InscribirseEnExamenAutogestUseCase`](backend/src/Application/Inscripciones/InscribirseEnExamenAutogestUseCase.cs)
    (resuelve el estudiante desde el token, chequea encuesta pendiente, delega al UseCase
    administrativo) — mismo diseño que su equivalente de materias. **Verificado:** Dirección
    inscribe explícitamente a un estudiante por Id → `201`; el mismo estudiante autogestionándose
    en otro examen vía el endpoint viejo → `201`; Dirección probando el endpoint viejo (ahora
    Estudiante-only) → `403`.
13. ✅ **Correlatividad duplicada → 500** — `Application/Correlatividades/CrearCorrelativiadadUseCase.cs`.
    Se agregó un chequeo explícito reutilizando la lista de correlatividades que ya se traía para
    la detección de ciclos (sin query extra) — duplicado exacto → `409` con mensaje claro, en vez
    de depender del catch-all de `DbUpdateException` del Tier 1. **Verificado:** crear la misma
    correlatividad dos veces → `409 "Ya existe una correlatividad de este tipo entre esas dos
    materias."` (antes: `500` genérico).
14. ✅ **Eliminar Materia con `EspacioCurricular` asociado → 500** —
    `Application/Materias/EliminarMateriaUseCase.cs`. Se agregó el mismo tipo de validación que
    ya existía para Inscripción/Examen/HistorialAcademico (nuevo método
    `IEspacioCurricularRepository.ExistePorMateriaIdAsync`). **Verificado:** eliminar una materia
    con una cátedra asignada → `400 "...porque tiene cátedras asignadas..."` (antes: `500`).
15. ✅ **Race condition en generación de código de Materia (`MAT-XXX`)** —
    `Infrastructure/Persistence/Repositories/MateriaRepository.cs:36` (`MAX+1` sin lock).
    **Fix:** mismo patrón que las razas de cupo del Tier 2 — lectura del máximo + insert
    envueltos en `IUnitOfWork.EjecutarEnTransaccionAsync(..., IsolationLevel.Serializable, ...)`.
    **Verificado con concurrencia real:** 5 altas de materia disparadas en paralelo → 2 exitosas
    con códigos secuenciales correctos (`MAT-004`, `MAT-005`), 3 rechazadas limpiamente con `409`
    (conflicto de serialización, sin duplicados ni 500); confirmado por SQL directo que no quedó
    ningún código repetido.

**Regresión:** hubo que actualizar `InscribirseEnExamenUseCaseTests.cs` (constructor cambiado y
DTO ahora recibe `EstudianteId` en vez de `UsuarioId`). **Suite completa: 142/142 passed.**

### Tier 5 — Validaciones y desalineaciones de RBAC/documentación ✅ REPARADO

**Decisión previa con el usuario sobre RBAC de Preceptor (afecta #17 y #21):** separar gestión de
identidad (cuentas de Estudiante — queda centralizada en Dirección) de logística de cursada
(inscripciones — se abre a Preceptor). Ver razonamiento completo más abajo en #17/#21.

16. ✅ **Sin validación de formato de DNI/Email en altas** — el backend solo validaba unicidad,
    no formato (`"ABC123XYZ"` como DNI o `"no-es-un-email"` como email daban `201 Created`).
    **Fix:** se movió la validación de formato al dominio —
    [Usuario.cs](backend/src/Domain/Entities/Usuario.cs) ahora valida DNI (solo dígitos, 7-10
    caracteres, mismo criterio que `PadronAlumno.Crear`) y Email (regex simple) tanto en `Crear`
    como en `Modificar`, aplicando a Docentes/Preceptores/Estudiantes/Usuarios por igual sin
    duplicar la regla en cada UseCase. **Verificado:** DNI inválido → `400`; email inválido →
    `400`; alta válida → `201` sin cambios.
17. ✅ **RBAC de `EstudiantesController` vs. `docs/casos-de-uso.md`** — la documentación decía que
    Preceptor podía crear/modificar/desactivar Estudiantes; el código lo restringía a Dirección.
    **Decisión (con el usuario):** esto es gestión de identidad (alta de cuenta con password,
    unicidad de DNI/Email) — se centraliza institucionalmente para tener un solo punto de
    auditoría y evitar que personal operativo cree/desactive cuentas. **Fix:** se corrigió
    [docs/casos-de-uso.md](docs/casos-de-uso.md) para reflejar que es Administrador-only (el
    código no cambió).
18. ✅ **`POST /api/auditoria/cambio-rol` era Direccion-only pero el frontend deja que un Docente
    lo dispare** — [AuditoriaController.cs](backend/src/Controllers/AuditoriaController.cs). El
    cambio de vista de un Docente a "Estudiante" (`AuthService.VISTAS_PERMITIDAS`) fallaba con
    403 en silencio, sin dejar rastro de auditoría. **Fix:** se amplió el endpoint a
    `Roles = "Direccion,Docente"` — los mismos roles que hoy tienen vistas habilitadas en el
    frontend. **Verificado:** Docente registra su cambio de vista → `204` (antes: 403 silencioso).
19. ✅ **CU-47: la validación de período de inscripción era global** — no filtraba por
    comisión/materia/curso, así que un solo período abierto habilitaba inscripción a todo el
    sistema. **Fix:** `MateriaId`/`CursoId` (ya soportados por la entidad `CalendarioAcademico`,
    solo faltaban en el DTO de alta) ahora se exponen en `CrearEventoCalendarioDto`; se actualizó
    `ICalendarioAcademicoRepository.EstaEnPeriodoAsync` para aceptarlos y matchear con lógica de
    wildcard: un evento sin materia/curso seteado sigue siendo un período global (compatibilidad
    con todo lo ya cargado), pero si los tiene, solo habilita esa materia/curso puntual. Aplicado
    en ambos call sites (`InscribirseEnMateriaUseCase`, `InscribirseEnExamenUseCase` — este último
    reordenado para tener el examen ya cargado y usar su `MateriaId` en el chequeo). **Verificado
    con ambos casos:** período acotado a otra materia → la inscripción sigue bloqueada (`400`,
    el bug persiste si no se configura bien); período acotado a la materia correcta → inscripción
    exitosa (`201`).
20. ✅ **"Cierre de actas" solo existía para Dirección, no para Preceptor** — CLAUDE.md exige
    explícitamente esto para Preceptores (CU-22/CU-33). **Fix:**
    [CursosController.cs](backend/src/Controllers/CursosController.cs), `PATCH /{id}/cerrar`
    ampliado a `Roles = "Direccion,Preceptor"`. **Verificado:** Preceptor cierra un curso → `204`
    (antes: `403`).
21. ✅ **Dar de baja de inscripción a materia e inscripción manual eran Direccion-only,
    contradiciendo `docs/casos-de-uso.md`** — la documentación decía "Preceptor, Administrador"
    para inscripción manual y "Estudiante, Preceptor" para dar de baja. **Decisión (con el
    usuario):** a diferencia de la gestión de cuentas (#17), esto es logística de cursada —
    operativo, reversible, no toca identidad — se alinea el código a la documentación. **Fix:**
    `POST /api/inscripciones/materias` ampliado a `Direccion,Preceptor`;
    `DELETE /api/inscripciones/materias/{id}` ampliado a `Direccion,Preceptor,Estudiante`, con un
    chequeo de propiedad nuevo en
    [DarDeBajaInscripcionMateriaUseCase.cs](backend/src/Application/Inscripciones/DarDeBajaInscripcionMateriaUseCase.cs)
    (un Estudiante solo puede dar de baja su propia inscripción — si no, sería un IDOR nuevo).
    **Verificado:** Preceptor da de baja cualquier inscripción → `204`; un Estudiante intentando
    dar de baja la inscripción de otro → `403 "No podés dar de baja la inscripción de otro
    estudiante."`; el mismo estudiante dando de baja la propia → `204`.
22. ✅ **`ModificarEstudianteUseCase` permitía forzar `Egresado`/`Desertor` sin validar** — desde
    el formulario genérico de edición se podía saltear por completo la máquina de estados de
    CU-43. **Fix:** se agregaron `ValidarPuedeEgresarAsync` (mismo criterio que
    `ActualizarEstadoAcademicoUseCase.EvaluarEgresoAsync`: 100% del plan de la carrera aprobado) y
    `ValidarPuedeDesertarAsync` (mismos criterios: sin inscripciones activas y sin actividad en
    los últimos 2 años) en
    [ModificarEstudianteUseCase.cs](backend/src/Application/Estudiantes/ModificarEstudianteUseCase.cs) —
    esto convierte la operación en "confirmar manualmente lo que ya debería ser cierto", no un
    override arbitrario. **Verificado con los 3 casos:** marcar Egresado sin materias aprobadas →
    `409` con el conteo real (`0/0`); marcar Desertor con una inscripción activa → `409`; marcar
    Desertor sin inscripciones activas ni actividad registrada → permitido (mismo criterio que el
    evaluador automático original, que también trata "sin actividad nunca registrada" como
    elegible).
23. ✅ **`EspacioCurricular` no validaba que Materia y Curso fueran de la misma Carrera** —
    [CrearEspacioCurricularUseCase.cs](backend/src/Application/EspaciosCurriculares/CrearEspacioCurricularUseCase.cs).
    **Verificado:** cátedra cruzando Carrera 1 (materia) con Carrera 2 (curso) → `400 "La materia
    y el curso pertenecen a carreras distintas."`
24. ✅ **Se podía crear una cátedra sobre un Curso ya `Cerrado`** — mismo archivo, se agregó
    chequeo de `curso.Estado != EstadoCurso.Activo`. **Verificado:** cátedra sobre curso cerrado
    → `409 "...el curso se encuentra en estado 'Cerrado'."`
25. ✅ **Correlatividades no generaban registro de auditoría** — se agregó
    `auditoria.RegistrarAsync(...)` a
    [CrearCorrelativiadadUseCase.cs](backend/src/Application/Correlatividades/CrearCorrelativiadadUseCase.cs)
    y [EliminarCorrelativiadadUseCase.cs](backend/src/Application/Correlatividades/EliminarCorrelativiadadUseCase.cs),
    mismo patrón que Materias/Cursos/EspaciosCurriculares.

**Regresión:** suite completa **142/142 passed**, sin necesidad de tocar tests existentes (los
cambios de este tier no afectaron ninguna firma cubierta por tests unitarios).

### Tier 6 — Funcionalidad documentada pero no construida

26. ⏸️ **No aplica — decisión del usuario.** `CarrerasController` no tiene CRUD real (solo
    `GET`); CU-02 lo esperaría, pero el plan institucional real solo contempla las 2 carreras ya
    cargadas (seed), sin necesidad de crear más desde la aplicación. No se construye CRUD para
    una necesidad que no existe. Se mantiene documentado acá por si la situación institucional
    cambia en el futuro.
27. ✅ **REPARADO — Alertas de "plazo de carga de notas" nunca llegaban al docente destinatario.**
    La lógica en
    [NotificarVencimientosUseCase.cs](backend/src/Application/Alertas/NotificarVencimientosUseCase.cs)
    ya usaba `evento.MateriaId`/`CursoId` correctamente para resolver el `EspacioCurricular` y
    notificar al docente — el único problema era que `CrearEventoCalendarioDto` no exponía esos
    campos, así que quedó resuelto como efecto colateral directo del fix del Tier 5 #19. Se
    completó agregando los campos también en el **frontend**
    ([crear-evento.component.ts/html](frontend/src/app/features/calendario/crear-evento/)):
    selects de Materia/Curso que aparecen solo cuando el tipo de evento es "Fecha límite carga de
    notas". **Verificado end-to-end:** evento creado con Materia+Curso → `POST /api/alertas/vencimientos`
    → el docente de esa cátedra aparece en `detalles` y recibe la notificación in-app real
    (confirmado con `GET /api/notificaciones` del docente).
28. ⏸️ **Pendiente — decisión del usuario, fuera de alcance por ahora.** Certificados con Hash
    SHA-256/QR (CLAUDE.md, módulo 6) — no hay código que los implemente. Es una funcionalidad
    nueva y grande (librería de QR, definición de qué certificado se emite, endpoint de
    verificación), no un bug — se prioriza aparte cuando corresponda.
29. ✅ **Regla anti-PII del Asistente IA sin ningún test que la protegiera a futuro** — se agregó
    [PreguntarAsistenteUseCaseAntiPiiTests.cs](backend/tests/AsistenteIA/PreguntarAsistenteUseCaseAntiPiiTests.cs):
    invoca por reflection cada una de las 11 herramientas devueltas por `ConstruirHerramientas()`
    (igual que lo haría el SDK de tool-calling en producción, incluyendo la resolución automática
    de argumentos desde el JSON Schema de cada herramienta) contra datos sembrados con marcadores
    de PII reconocibles, y falla si el resultado serializado de cualquiera los contiene.
    **Verificación de que el test realmente protege algo** (no es un test vacío): se removió
    temporalmente la llamada a `ProyectarRiesgo(...)` en `riesgo_academico` → el test detectó la
    fuga real (mostró el JSON con legajo y nombre completo expuestos) → se restauró el código
    original → el test volvió a pasar limpio. **Suite completa: 142/142 passed** (141 previos +
    este nuevo).

### Tier 7 — Cosméticos / naming / UX menor ✅ REPARADO

30. ✅ **`mis-examenes.component.ts` sin el fix fail-open de encuesta** —
    [mis-examenes.component.ts](frontend/src/app/features/mis-examenes/mis-examenes.component.ts).
    Mismo fix que `ffe69ff` aplicó en `mis-materias.component.ts`: si falla la verificación de
    encuesta pendiente, ya no se intenta la inscripción igual — se muestra un error y se deja
    reintentar.
31. ✅ **`Curso.Crear` con los campos `Anio`/`AnioLectivo` semánticamente invertidos** —
    en vez de renombrar propiedades (tocaría entidad + migración + DTOs + frontend en todo el
    sistema para un problema que es solo de claridad, no funcional), se documentó explícitamente
    con XML doc comments en
    [Curso.cs](backend/src/Domain/Entities/Curso.cs) y
    [CrearCursoDto.cs](backend/src/Application/Cursos/DTOs/CrearCursoDto.cs), y se mejoraron los
    mensajes de excepción para que digan "año calendario"/"año de cursada" en vez de repetir el
    nombre ambiguo del parámetro.
32. ✅ **`CrearCursoDto.PreceptorId` en realidad esperaba el `UsuarioId` del preceptor** —
    renombrado a `PreceptorUsuarioId` en
    [CrearCursoDto.cs](backend/src/Application/Cursos/DTOs/CrearCursoDto.cs) y
    [ModificarCursoDto.cs](backend/src/Application/Cursos/DTOs/ModificarCursoDto.cs) (la salida,
    `CursoDto.PreceptorId`, sí representa el id real de la entidad — no se tocó, no tenía el
    problema). Actualizado en ambos UseCases y en el frontend
    ([cursos.service.ts](frontend/src/app/features/cursos/cursos.service.ts) y los dos
    componentes que arman el request). **Verificado:** alta de curso con `preceptorUsuarioId` en
    el body → `201`.
33. ✅ **Backend aceptaba contraseñas de 6 caracteres, frontend exige 8+** — se centralizó en
    `Usuario.ValidarFortalezaPassword` (mínimo ahora 8, igual que el frontend), reemplazando la
    validación duplicada a mano en 6 lugares (`RegistroUseCase`, `CrearUsuarioUseCase`,
    `CrearDocenteUseCase`, `CrearPreceptorUseCase`, `CrearEstudianteUseCase`,
    `CambiarClaveUseCase`). **Verificado:** password de 6 caracteres → `400`; de 8 → `201`.
34. ✅ **`CarrerasService.carreras$` era `readonly`** — cambiado al patrón lazy exacto que
    documenta CLAUDE.md (`cache$: Observable<Carrera[]> | null`) en
    [carreras.service.ts](frontend/src/app/features/carreras/carreras.service.ts). No hay
    mutaciones que invalidarlo todavía (ver Tier 6 #26), pero ya no bloquearía compilar si se
    agregan en el futuro.
35. ✅ **Varios controllers documentaban `404 NotFound` en Swagger pero devolvían `400`** —
    corregidas las anotaciones `[ProducesResponseType]` en los 7 endpoints reales de
    `MateriasController`, `CursosController`, `CorrelativiadadesController` y
    `EspaciosCurricularesController` cuyo UseCase subyacente lanza `BusinessException` (400) y
    no `KeyNotFoundException` (404). Solo se tocó la anotación de Swagger — el comportamiento
    real (ya `400`) no cambió.
36. ✅ **`InscripcionMateriaResultDto.materiaNombre` viajaba vacío** —
    [InscribirseEnMateriaUseCase.cs](backend/src/Application/Inscripciones/InscribirseEnMateriaUseCase.cs)
    usaba `inscripcion.Materia?.Nombre` sobre una entidad recién creada sin esa navegación
    cargada; se cambió a usar la navegación ya resuelta del `EspacioCurricular` validado en el
    paso anterior. **Verificado:** respuesta de alta ahora trae el nombre real de la materia.
37. ✅ **`RestablecerPasswordUseCase` no validaba fortaleza de la nueva contraseña** — se agregó
    `Usuario.ValidarFortalezaPassword` (mismo fix que #33) a
    [RestablecerPasswordUseCase.cs](backend/src/Application/Auth/RestablecerPasswordUseCase.cs).
38. ✅ **Sin capacidad de cierre remoto de sesión** — el ítem más grande del tier. Se agregó
    [`ISesionService.ForzarCierre`/`FueForzadoDespuesDe`](backend/src/Application/Interfaces/ISesionService.cs)
    y un endpoint `DELETE /api/sesiones/activas/{usuarioId}` (Direccion-only) en
    [SesionesController.cs](backend/src/Controllers/SesionesController.cs). El chequeo se hace en
    `OnTokenValidated` ([Program.cs](backend/src/Program.cs)), comparando el claim `iat` del JWT
    contra el momento del cierre forzado — un login posterior emite un token nuevo y vuelve a
    funcionar. **Dos bugs reales encontrados y corregidos en el camino, ambos solo visibles al
    probar con curl de verdad, no en el papel:**
    - El JWT **no tenía claim `iat` en absoluto** (confirmado decodificando un token real) — el
      chequeo de "¿es anterior al cierre forzado?" nunca se disparaba, así que ningún token
      quedaba invalidado. Se agregó explícitamente en
      [JwtService.cs](backend/src/Infrastructure/Auth/JwtService.cs).
    - `iat` tiene precisión de segundos enteros (spec JWT), pero el timestamp del cierre forzado
      se guardaba con sub-segundos — en pruebas rápidas (mismo segundo) la comparación se
      invertía (rechazaba el token nuevo y dejaba pasar el viejo). Se truncó a segundos en
      [SesionService.cs](backend/src/Infrastructure/Sesiones/SesionService.cs) y se usó `>=` en
      vez de `>` (favorece exigir re-login en el caso ambiguo del mismo segundo).
    **Verificado end-to-end con margen de tiempo entre pasos:** token funciona (`200`) → Dirección
    cierra la sesión (`204`) → el mismo token viejo → `401` → login nuevo 2s después → `200`.
39. ✅ **Mensaje de error confuso ("El usuario no es un usuario")** —
    [CambiarActivacionUseCase.cs](backend/src/Application/Usuarios/CambiarActivacionUseCase.cs)
    ahora dice explícitamente el rol esperado y el rol real del usuario en vez de reusar el
    nombre de la entidad de forma que a veces no tenía sentido gramatical.

**Regresión:** backend 142/142 tests; frontend `tsc --noEmit` sin errores en ambas rondas
(cambios de componentes + servicios).

---

## Próximo paso

Arrancar por el Tier 1 (los dos causa-raíz) desbloquea o simplifica gran parte de lo que sigue.
Ningún fix se aplicó todavía — esto es el plan, pendiente de confirmación para empezar a
implementar.
