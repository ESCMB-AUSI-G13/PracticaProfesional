# Revisión visual (frontend) — post-reparación de 39 hallazgos

Plan para repasar a mano, haciendo click en el navegador, los 35 fixes reparados en esta sesión
(ver [CHECKLIST.md](CHECKLIST.md), Tiers 1–7). Se hace **contra Azure SQL real** (decisión del
usuario), así que cada sección marca qué es solo lectura (seguro) y qué escribe datos reales
(⚠️ **usar cuenta de prueba**, nunca una cuenta real de la institución).

No repite los 4 ítems no aplicables al frontend: #26 (no aplica, decisión del usuario), #28
(diferido, decisión del usuario) — ninguno tiene UI que revisar hoy.

---

## 0. Antes de empezar

### Reglas de oro (Azure real, sin red de seguridad de un sandbox descartable)

1. **Nunca ejecutes una acción destructiva/masiva sobre una cuenta real** (desactivar, forzar
   cierre de sesión, cambiar rol) sin haber creado antes una cuenta de prueba para probarla ahí.
2. **Nunca desactives cuentas de Dirección hasta dejar cero activas.** El fix del ítem #8 debería
   bloquearlo con `409`, pero no vale la pena arriesgarse a confirmarlo contra la única cuenta
   real de Dirección — probalo con dos cuentas de Dirección de prueba (ver abajo).
3. **El envío de recuperación de contraseña (`/forgot-password`) manda un email real** (Azure
   Communication ya está configurado en producción, ver CLAUDE.md). Usá una casilla que controles
   vos, no la de otra persona.
4. **El registro autogestionado (`/registro`) crea un Usuario + Estudiante reales en Azure SQL.**
   Solo probalo con un DNI de prueba que vos mismo cargues al Padrón antes.

### Cuentas de prueba a crear primero (como Dirección, desde `/usuarios/nuevo`, `/docentes/nuevo`, etc.)

Nombralas con un prefijo bien visible para no confundirlas nunca con datos reales, por ejemplo
`PRUEBA - Borrar`, y anotá los DNI/legajos usados para poder encontrarlas después en la limpieza:

| Rol | Para qué se usa en este plan |
|---|---|
| Dirección (2ª cuenta) | Probar el guard de "no desactivar al último Dirección" (#8) sin tocar tu cuenta real |
| Preceptor | Recorrer las pantallas de Preceptor sin usar una cuenta real |
| Docente | Cargar notas/asistencias de prueba, probar cambio de vista (#18) |
| Estudiante (2 cuentas) | Probar inscripciones, IDOR de baja de inscripción (#21), revocación de JWT (#9) |

---

## 1. Autenticación y cuentas — cualquier rol

Todo esto se prueba **sin usar tu cuenta real** salvo que se indique lo contrario.

- [ ] **Login con credenciales inválidas** (`/login`) — password incorrecta y usuario inexistente
      dan el mismo mensaje genérico, sin filtrar cuál dato estaba mal.
- [ ] **Rate limiting de login (#10)** — con la cuenta de prueba, escribí mal la contraseña 8+
      veces seguidas en menos de 5 minutos. A partir del 8°/9° intento debería cortar con un error
      claro de "demasiados intentos" en vez de seguir devolviendo error de credenciales. ⚠️ Te vas
      a autobloquear 5 minutos en esa cuenta — normal, es el fix funcionando.
- [ ] **Login con usuario inactivo** — desactivá momentáneamente la cuenta de prueba Docente desde
      `/docentes`, intentá loguearte con ella → mensaje claro de cuenta desactivada. Reactivala
      después.
- [ ] **Formato de contraseña (#33/#37)** — en `/registro`, `/usuarios/nuevo` o
      `/reset-password`, probá una contraseña de 6 caracteres → debe rechazarla (mínimo ahora 8 en
      frontend y backend, ya no hay desalineación).
- [ ] **Recuperar contraseña sin filtrar existencia (#11)** — en `/forgot-password`, probá con un
      email que **no existe** en el sistema → mismo mensaje genérico de siempre (`200`), sin
      error. ⚠️ Si además probás con un email que sí existe, va a llegar un correo real — usá el
      de tu cuenta de prueba.
- [ ] **Revocación de JWT al desactivar (#9)** — abrí una sesión con la cuenta de prueba Docente
      en una ventana (o navegador) aparte. Con tu cuenta real de Dirección, desactivá esa cuenta
      desde `/docentes`. Volvé a la sesión del Docente y navegá a cualquier pantalla → debería
      cortar la sesión (401/redirect a login) al instante, sin esperar a que expire el token.
- [ ] **Registro autogestionado (#Grupo1, con padrón)** — cargá un DNI de prueba en `/padron`
      (como Dirección), después andá a `/registro` (deslogueado) y registrate con ese DNI. Debe
      crear una cuenta Estudiante nueva. ⚠️ Esto crea datos reales — usá un DNI claramente de
      prueba.
- [ ] **Autodesactivación del último Dirección (#8)** — con tu 2ª cuenta de prueba de Dirección
      creada arriba: desactivá todas las demás cuentas de Dirección de prueba hasta que quede solo
      esa una activa (nunca toques la cuenta real), después intentá desactivarla también →
      `409` con mensaje explícito ("es el único usuario de Dirección activo..."). No debería
      dejarte en 0.

---

## 2. Dirección

Iniciá sesión con tu cuenta real de Dirección para lo que es solo lectura; usá las cuentas de
prueba para lo que escribe datos.

### Identidad y validaciones (solo lectura salvo que se aclare)

- [ ] **Validación de formato DNI/Email (#16)** — en `/estudiantes/nuevo` (o `/docentes/nuevo`),
      probá un DNI tipo `"ABC123XYZ"` y un email tipo `"no-es-un-email"` → ambos deben rechazarse
      con `400` antes de llegar a guardar nada. ⚠️ Completá el resto del formulario con datos de
      prueba si querés confirmar también el alta válida.
- [ ] **Mensaje de error mejorado (#39)** — difícil de forzar solo desde la UI (antes decía "el
      usuario no es un usuario"); si en algún punto ves un error de activación/rol, debería
      nombrar explícitamente el rol esperado vs. el real. No es necesario forzarlo a propósito.

### Estructura académica (usar materias/cursos/correlatividades **de prueba**, no reales)

- [ ] **Materia duplicada / código autogenerado (#15)** — en `/materias/nueva`, dá de alta una
      materia de prueba → confirmá que el código (`MAT-XXX`) sale correlativo sin duplicados.
- [ ] **Eliminar materia con cátedra asignada (#14)** — creá una materia y un
      `EspacioCurricular` de prueba sobre ella (`/espacios-curriculares`), después intentá
      eliminar la materia desde `/materias` → debe rechazar con `400` claro ("...porque tiene
      cátedras asignadas..."), no un error genérico.
- [ ] **Correlatividad duplicada (#13)** — en la vista de correlatividades de una materia, creá la
      misma correlatividad dos veces seguidas → la segunda debe dar `409` con mensaje claro, no
      un error genérico ni pantalla rota.
- [ ] **Auditoría de correlatividades (#25)** — después del paso anterior, andá a `/auditoria` y
      confirmá que aparece un registro por la creación de la correlatividad.
- [ ] **Cátedra cruzando carreras (#23)** — en `/espacios-curriculares`, intentá crear una cátedra
      combinando una Materia de una carrera con un Curso de otra carrera → `400` explícito ("La
      materia y el curso pertenecen a carreras distintas").
- [ ] **Cátedra sobre curso cerrado (#24)** — cerrá un curso de prueba (`/cursos`, botón
      cerrar), después intentá crear una cátedra sobre ese curso ya cerrado → `409` explícito.
- [ ] **Cierre de curso, doble clic (#7)** — sobre el mismo curso de prueba ya cerrado, intentá
      cerrarlo otra vez → `409` ("El curso ya está cerrado"), ya no lo acepta en silencio.

### Calendario académico y alertas (#19, #27)

- [ ] En `/calendario/nuevo`, elegí el tipo de evento "Fecha límite carga de notas" → deberían
      aparecer selects de **Materia** y **Curso** que antes no estaban (fix de frontend de este
      tier). Guardalo apuntando a una cátedra donde tengas un Docente de prueba asignado.
- [ ] Con la cuenta Docente de prueba (esa cátedra), revisá sus notificaciones → debería haber
      recibido el aviso de vencimiento (antes nunca llegaba porque el DTO no viajaba con
      materia/curso).
- [ ] **Período acotado por materia (#19)** — creá un evento de "período de inscripción" limitado
      a una materia/curso específico. Confirmá que un Estudiante de prueba puede inscribirse a esa
      materia puntual, pero sigue bloqueado si el período no aplica a otra materia distinta.

### Estudiantes — máquina de estados (usar el Estudiante **de prueba**)

- [ ] **Forzar Egresado/Desertor sin cumplir criterios (#22)** — en
      `/estudiantes/:id/editar` del estudiante de prueba (sin materias aprobadas), intentá
      cambiarle el estado a "Egresado" a mano → `409` con el conteo real (ej. "0/0 materias
      aprobadas"), ya no lo acepta silenciosamente.
- [ ] Repetí con "Desertor" teniendo una inscripción activa → `409`. Dando de baja esa
      inscripción primero y reintentando sin actividad reciente → ahora sí lo permite.

### Reportes / Dashboards (solo lectura)

- [ ] `/reportes/tablero-ejecutivo`, `/reportes/riesgo-academico`, `/reportes/retencion-cohorte`,
      `/reportes/retencion-anual`, `/reportes/desercion-por-anio`,
      `/reportes/egresados-por-carrera`, `/reportes/encuestas-satisfaccion`,
      `/reportes/encuestas-comparativo` — abrí cada uno y confirmá que cargan con números
      reales (0–100% en tasas, sin `NaN` ni negativos). Esto es regresión general: las tablas de
      Encuestas que el Tier 1 reparó alimentan varios de estos reportes.
- [ ] Exportación a PDF desde alguno de estos paneles → se descarga un PDF válido.

### Sesiones (⚠️ funcionalidad sin pantalla propia todavía)

- [ ] **No hay una pantalla dedicada en el frontend para "cerrar sesión remota" (#38).** El
      endpoint `DELETE /api/sesiones/activas/{usuarioId}` existe y funciona (verificado por API
      en esta sesión), pero no hay botón en ninguna ruta listada en
      [app.routes.ts](frontend/src/app/app.routes.ts) que lo dispare. Si querés confirmarlo
      visualmente de todos modos, es la única excepción a "todo por click": haría falta Postman o
      la consola del navegador. Si no, quedá tranquilo con la verificación ya hecha por API.

---

## 3. Preceptor

Iniciá sesión con la cuenta Preceptor de prueba.

- [ ] `/asistencias/rectificar` — rectificar una asistencia sin motivo debería exigirlo; con
      motivo, genera una fila nueva en auditoría (visible después como Dirección en `/auditoria`).
- [ ] `/reportes/inasistencias` y `/reportes/control-legajo` — cargan datos reales, sin errores de
      permisos.
- [ ] `/calendario` — puede ver el calendario (solo lectura, no tiene botón de crear evento).

### ⚠️ Gap de UI conocido (no es un bug de esta revisión, es una decisión pendiente)

Los ítems #20 (cierre de actas) y #21 (alta/baja de inscripción a materia) ampliaron el **backend**
para permitir Preceptor, pero las rutas del frontend `cursos` e `inscripciones-materia` siguen
gateadas a `roleGuard('Direccion')` únicamente en
[app.routes.ts](frontend/src/app/app.routes.ts) — como Preceptor **no vas a ver esas pantallas en
el menú**, aunque el backend ya te dejaría usarlas. No hay nada que verificar visualmente acá
todavía; si querés que el Preceptor pueda usarlas de verdad, es un cambio de una línea en las
rutas del frontend que puedo hacer en otra sesión si lo pedís.

---

## 4. Docente

Iniciá sesión con la cuenta Docente de prueba.

- [ ] `/calificaciones/carga-notas` — cargar una nota fuera de rango (0, 11, -1) → rechazada;
      nota=4 → Aprobada; nota=3.99 → Desaprobada; nota con más de 2 decimales → se redondea.
- [ ] Rectificar una nota que no esté en estado Aprobada/Desaprobada → debería bloquearlo (la
      máquina de estados del dominio, no solo el formulario).
- [ ] `/asistencias/registrar` — registrar la misma asistencia dos veces (mismo alumno, misma
      fecha, misma cátedra) → `400` limpio, no error genérico.
- [ ] **Cambio de vista + auditoría (#18)** — si tu usuario Docente tiene una vista alternativa
      habilitada (control de "ver como" en el shell/header), cambiala → ya no debería fallar en
      silencio. Como Dirección, confirmá en `/auditoria` que quedó registrado el cambio.
- [ ] `/mis-encuestas`, `/mis-encuestas/resultados`, `/mis-encuestas/comparativo` — cargan sin
      error, mostrando solo las cátedras propias del Docente.
- [ ] `/reportes/comisiones`, `/reportes/evolucion`, `/reportes/catedras` — cargan con datos
      reales, acotados a las cátedras del Docente.

---

## 5. Estudiante

Iniciá sesión con una cuenta Estudiante de prueba (usá la segunda para el chequeo de IDOR).

- [ ] `/mis-materias` — inscribite a una materia con correlatividades cumplidas → `201` y el
      nombre de la materia aparece correcto en la confirmación (**#36** — antes viajaba vacío).
      Probá también una materia con correlatividades incumplidas → bloqueada con mensaje claro.
- [ ] **Encuesta obligatoria** — si tenés una encuesta pendiente, el sistema debe exigir
      completarla antes de dejarte inscribir (gate ya validado por código en esta sesión —
      confirmalo una vez desde la UI).
- [ ] `/mis-examenes` — inscribite a un examen → mismo chequeo de encuesta pendiente y de
      correlatividades (esta vez para rendir: "Aprobado" en la cursada, no solo "Regularizado").
- [ ] **Baja de inscripción propia (#21)** — dar de baja tu propia inscripción a una materia →
      `204`, sin problema.
- [ ] **IDOR de baja de inscripción (#21, con la 2ª cuenta de Estudiante)** — desde la cuenta
      Estudiante B, intentá dar de baja (por API/URL manipulada, o si el frontend expone un id) la
      inscripción de la cuenta Estudiante A → debería rechazar con `403` explícito ("No podés dar
      de baja la inscripción de otro estudiante"). Este paso es más de "confirmar que no se puede"
      que un flujo de click normal — opcional si no tenés forma fácil de intentarlo desde la UI.
- [ ] `/mis-encuestas-pendientes` — responder una encuesta pendiente, confirmar que desaparece de
      la lista después.
- [ ] `/calendario` — solo lectura, sin botón de crear evento.

---

## 6. Asistente de IA (solo Dirección)

Con tu cuenta real de Dirección: abrí el botón flotante de chat y hacé una pregunta que dispare
alguna herramienta con datos por alumno, por ejemplo *"¿qué estudiantes están en riesgo académico
este cuatrimestre?"* o *"dame el detalle de inasistencias"*.

- [ ] **Anti-PII (#29)** — la respuesta debe hablar en **totales agregados** (cantidades,
      porcentajes, promedios) y **nunca** nombrar un alumno, legajo o DNI puntual. Este es
      justamente el comportamiento que el nuevo test automatizado protege — la confirmación visual
      es solo un plus, no la única red de seguridad.
- [ ] El botón flotante no debería aparecer en absoluto con otro rol logueado (Preceptor/Docente/
      Estudiante) — confirmalo de paso en cualquiera de las secciones anteriores.

---

## 7. Limpieza final

Antes de cerrar la revisión, borrá o desactivá todo lo que creaste para probar, para no dejar
basura en la base real:

- [ ] Desactivar (o eliminar si el sistema lo permite) las cuentas `PRUEBA - Borrar` de los 4
      roles, incluida la 2ª cuenta de Dirección.
- [ ] Eliminar la materia/curso/cátedra/correlatividad de prueba creadas en la sección 2, en este
      orden (por las validaciones de integridad referencial que este mismo review reparó, algunas
      no se van a dejar borrar si tienen dependencias — empezá por lo más "hijo"): correlatividad
      → cátedra (`EspacioCurricular`) → inscripciones de prueba → curso → materia.
- [ ] Sacar del Padrón el DNI de prueba usado para el registro autogestionado, si ya no hace
      falta.
- [ ] Revisar `/auditoria` una última vez — todo lo de arriba debería haber quedado registrado; es
      una buena confirmación cruzada de que el módulo de auditoría (CU-06) sigue funcionando bien
      con datos reales.

---

## Referencia rápida — qué ítem del CHECKLIST corresponde a qué sección

| Ítems | Dónde probarlos |
|---|---|
| #7, #8, #9, #10, #11, #16, #33, #37, #39 | Sección 1 (Auth) y 2 (Dirección) |
| #13, #14, #15, #23, #24, #25 | Sección 2 (Dirección → Estructura académica) |
| #19, #27 | Sección 2 (Dirección → Calendario) |
| #22 | Sección 2 (Dirección → Estudiantes) |
| #18 | Sección 4 (Docente) + confirmación en Auditoría |
| #20, #21 | Secciones 2/3/5 (nota el gap de UI de Preceptor) |
| #29 | Sección 6 (Asistente IA) |
| #36 | Sección 5 (Estudiante) |
| #38 | Sección 2, sin UI propia — verificación ya hecha por API |
| #1, #2, #3, #4, #5, #6, #12, #17 | Causas raíz / backend puro, ya verificados por API+tests en esta sesión; no requieren click-through adicional |
| #26, #28 | No aplica / diferido — nada que revisar |
