# Manual de Usuario
### Sistema de Gestión Académica — Instituto Superior del Profesorado en Ciencias Económicas y Jurídicas

**URL del sistema:** https://escmbpracticaprof.z13.web.core.windows.net

---

## Índice

1. [Acceso al sistema](#1-acceso-al-sistema)
2. [Administrador / Dirección](#2-administrador--dirección)
   - [2.1 Gestión de usuarios](#21-gestión-de-usuarios)
   - [2.2 Gestión académica](#22-gestión-académica)
   - [2.3 Reportes](#23-reportes)
   - [2.4 Alertas académicas](#24-alertas-académicas)
   - [2.5 Encuestas académicas](#25-encuestas-académicas)
   - [2.6 Auditoría](#26-auditoría)
3. [Docente](#3-docente)
   - [3.1 Mis materias](#31-mis-materias)
   - [3.2 Registro de asistencias](#32-registro-de-asistencias)
   - [3.3 Carga de notas de examen](#33-carga-de-notas-de-examen)
   - [3.4 Encuestas de evaluación](#34-encuestas-de-evaluación)
4. [Preceptor](#4-preceptor)
5. [Estudiante](#5-estudiante)
   - [5.1 Inscripción a materias](#51-inscripción-a-materias)
   - [5.2 Encuesta pendiente](#52-encuesta-pendiente)
   - [5.3 Inscripción a exámenes finales](#53-inscripción-a-exámenes-finales)

---

## 1. Acceso al sistema

### Iniciar sesión

1. Ingresá a la URL del sistema.
2. Completá tu **Legajo** y **Contraseña**.
3. Hacé clic en **Ingresar**.

El sistema te redirige automáticamente al panel correspondiente a tu rol.

### Recuperar contraseña

1. En la pantalla de login, hacé clic en **¿Olvidaste tu contraseña?**
2. Ingresá tu dirección de email institucional.
3. Recibirás un email con un enlace para restablecer tu contraseña. El enlace es válido por **1 hora**.
4. Hacé clic en el enlace, ingresá tu nueva contraseña y confirmala.

> El email de recuperación llega desde `DoNotReply@azurecomm.net`. Si no lo encontrás en la bandeja de entrada, revisá la carpeta de **spam**.

### Cambiar contraseña

Desde cualquier rol: hacé clic en tu nombre en la esquina superior derecha → **Cambiar contraseña**.

---

### Notificaciones internas

El sistema genera notificaciones internas cada vez que se dispara una alerta académica o de vencimiento. Aparecen en la **campanita** ubicada en la barra lateral, junto a tu nombre y rol.

**Cómo usarlas:**

- El **badge rojo** sobre la campanita indica la cantidad de notificaciones sin leer.
- Hacé clic en la campanita para abrir el panel de notificaciones.
- Cada notificación muestra su título, mensaje y fecha.
- Hacé clic en una notificación para marcarla como leída.
- Usá el botón **Marcar todas como leídas** para limpiar el panel de una vez.
- El panel se cierra automáticamente al hacer clic fuera de él.
- El sistema actualiza las notificaciones automáticamente cada 60 segundos.

> Las notificaciones internas complementan los emails: si el email falla, la notificación en el sistema siempre se genera igual.

---

## 2. Administrador / Dirección

El rol **Administrador** (Dirección) tiene acceso completo al sistema. Desde el panel puede gestionar todos los recursos académicos y consultar reportes globales.

---

### 2.1 Gestión de usuarios

#### Crear un usuario

1. Ir a **Usuarios** en el menú lateral.
2. Seleccionar la pestaña correspondiente: **Docentes**, **Preceptores** o **Estudiantes**.
3. Hacer clic en **Nuevo**.
4. Completar el formulario:
   - **DNI** — debe ser único en el sistema.
   - **Legajo** — se genera automáticamente, pero puede editarse.
   - **Email institucional** — debe ser único.
   - **Nombre y Apellido**.
   - Datos específicos del rol (ej. Categoría para docentes, Año y Carrera para estudiantes).
5. Hacer clic en **Guardar**. El sistema envía las credenciales al email del usuario.

#### Modificar un usuario

1. En la lista de usuarios, hacer clic en el ícono de edición (lápiz) del usuario.
2. Editar los campos necesarios.
3. Guardar los cambios. El sistema registra el cambio en el **log de auditoría** automáticamente.

#### Desactivar / reactivar un usuario

- **Desactivar:** clic en el ícono de baja (tachado) → confirmar. El usuario no podrá iniciar sesión, pero su historial se conserva.
- **Reactivar:** filtrar por "Inactivos" → clic en el ícono de reactivación.

> La baja es **lógica** (soft delete): los datos nunca se eliminan de la base de datos.

---

### 2.2 Gestión académica

#### Carreras y materias

1. Ir a **Plan Académico** → **Carreras** para ver las carreras disponibles.
2. En **Materias**, podés crear nuevas materias asignándolas a una carrera y definiendo su código único.
3. Para definir qué materias son prerequisito de otras, ir a **Correlatividades**:
   - Seleccionar la materia destino (la que se quiere cursar/rendir).
   - Seleccionar la materia requisito.
   - Indicar si el requisito es para **cursar** (requiere regularizada) o para **rendir** (requiere aprobada).

#### Cursos

Los cursos representan una comisión de un año lectivo.

1. Ir a **Cursos** → **Nuevo curso**.
2. Completar: Año lectivo, Año (1°, 2°, 3°...), Comisión, Cupo y Preceptor asignado.
3. El curso se crea en estado **Activo**.
4. Al finalizar el año lectivo, usar **Cerrar curso** para inhabilitar nuevas inscripciones.

#### Espacios curriculares

Un espacio curricular asocia un **docente** a una **materia** dentro de un **curso**.

1. Ir a **Espacios Curriculares** → **Nuevo**.
2. Seleccionar Materia, Docente y Curso.
3. Guardar. El docente ahora verá esa materia en su panel.

#### Calendario académico

Define los períodos oficiales del año lectivo (inicio de clases, períodos de examen, fechas límite de carga de notas, etc.).

1. Ir a **Calendario** → **Nuevo evento**.
2. Completar: Nombre del evento, Tipo (ej. `FechaLimiteCargaNotas`, `InscripcionMateria`), Fecha inicio y fin.
3. Opcionalmente, asociar el evento a un curso y materia específicos.

> El sistema usa estas fechas para validar inscripciones y disparar alertas automáticas.

---

### 2.3 Reportes

Todos los reportes están en la sección **Reportes** del menú lateral.

#### Tablero ejecutivo institucional (RR-01)

Vista de resumen global para la dirección. Se carga automáticamente al ingresar a **Reportes → Tablero ejecutivo**.

Muestra:
- Métricas generales de la institución (total de estudiantes, tasa de egreso, deserción, etc.).
- **Gráfico de barras:** evolución por cohorte — cantidad de activos, egresados y desertores por año de ingreso.
- **Gráfico de dona:** distribución del riesgo académico actual (Alto / Medio / Bajo).
- Botón **Descargar PDF** para exportar el tablero completo.

---

#### Riesgo de deserción académica

Permite identificar estudiantes en situación de riesgo, con filtros y detalle individual.

1. Ir a **Reportes → Riesgo académico**.
2. Aplicar filtros opcionales: **Carrera**, **Año de cohorte**, **Nivel de riesgo** (Alto / Medio / Bajo).
3. Hacer clic en **Buscar**.
4. El panel muestra:
   - Gráfico de dona con la distribución de niveles de riesgo.
   - Tabla paginada con cada estudiante, su legajo, condición académica y nivel de riesgo.
5. Podés buscar un estudiante por nombre o legajo dentro de la tabla.
6. Usar **Descargar PDF** para exportar el listado con los filtros aplicados.

---

#### Retención y deserción por cohorte

Muestra cómo evoluciona cada cohorte a lo largo del tiempo: cuántos siguen activos, cuántos egresaron y cuántos desertaron.

1. Ir a **Reportes → Retención por cohorte**.
2. Aplicar filtros opcionales: **Carrera**, **Año de cohorte**.
3. Hacer clic en **Buscar**.
4. El panel tiene dos pestañas:
   - **Por cohorte:** gráfico de barras agrupado (Activos / Egresados / Desertores) por año de ingreso. Botón **Descargar PDF**.
   - **Retención anual:** tabla con tasas de retención por año ordinal de cursada (1°, 2°, 3°...) para cada cohorte. Botón **Descargar PDF**.

---

#### Reporte de inasistencias (RR-08)

Muestra el registro de asistencias con filtros por curso, materia, rango de fechas y si incluir solo ausencias.

- Usar los filtros para acotar la búsqueda.
- Hacer clic en **Generar reporte**.

#### Comparativo entre comisiones (RR-05)

Compara tasas de aprobación entre comisiones de un mismo año lectivo.

#### Evolución de notas por período (RR-06)

Muestra cómo evoluciona el promedio de una materia a lo largo de los períodos de examen.

#### Promedios por cátedra (RR-07)

Muestra el promedio de notas por materia y docente.

#### Control de legajo individual (RR-09)

Permite buscar un estudiante por legajo y ver todo su historial: materias cursadas, notas, asistencias y condición académica actual.

---

### 2.4 Alertas académicas

El sistema envía alertas automáticas **todos los lunes a las 08:00** a los destinatarios correspondientes. También podés ejecutarlas manualmente cuando sea necesario.

#### Detección de riesgo académico

Detecta estudiantes con riesgo de perder la regularidad o de abandonar.

**Criterios:**
- **Riesgo de asistencia:** más del 25% de ausencias injustificadas en alguna materia.
- **Riesgo de inactividad:** sin actividad registrada hace más de 30 días.

**Quién recibe el email:**
- El estudiante en riesgo.
- Todos los preceptores activos.
- Todos los usuarios con rol Administrador/Dirección.

**Ejecutar manualmente:** Ir a **Alertas** → **Detectar riesgo ahora**.

#### Notificación de vencimientos

Avisa sobre eventos del Calendario Académico que vencen en los próximos 3 días.

**Quién recibe el email:**
- `FechaLimiteCargaNotas`: el docente asignado al espacio curricular + preceptores + dirección.
- `InscripcionMateria` / `InscripcionExamen`: preceptores + dirección.

**Ejecutar manualmente:** Ir a **Alertas** → **Notificar vencimientos**.

#### Historial de alertas

Ir a **Alertas** → **Historial** para ver todas las alertas generadas, filtradas por tipo o estado de envío.

---

### 2.5 Encuestas académicas

#### Crear una encuesta

1. Ir a **Encuestas** en el menú lateral.
2. Hacer clic en **Nueva encuesta**.
3. Completar:
   - **Título** — nombre de la encuesta.
   - **Descripción** — opcional.
   - **Tipo** — `Evaluación Docente` (vinculada a una materia) o `Satisfacción General`.
   - **Ciclo lectivo** — año académico al que corresponde.
4. Guardar.

#### Agregar preguntas

1. Dentro de la encuesta creada, hacer clic en **Agregar pregunta**.
2. Completar:
   - **Texto** de la pregunta.
   - **Tipo** — `Escala Likert` (valores del 1 al 5) o `Texto libre` (respuesta abierta).
   - **¿Es obligatoria?**
3. Repetir para cada pregunta. El orden se asigna automáticamente.

#### Activar / desactivar una encuesta

- **Activar:** la encuesta queda disponible para ser asignada a los estudiantes.
- **Desactivar:** deja de mostrarse; las respuestas ya registradas se conservan.

> Una encuesta activa se presenta al estudiante como requisito previo al inscribirse a materias o exámenes. Solo se muestra si está activa.

#### Ver resultados (RR-03)

1. Ir a **Reportes → Encuestas de satisfacción**.
2. Seleccionar la encuesta a analizar.
3. El panel muestra:
   - Total de respuestas recibidas y promedio global.
   - Resultados por pregunta con gráfico de barras (el color indica el nivel: verde ≥ 4, naranja ≥ 3, rojo < 3).
   - Comentarios de preguntas de texto libre (expandibles).
   - Evolución mensual: respuestas y promedio por período.

#### Comparativo de encuestas (RR-04)

1. Ir a **Reportes → Comparativo de encuestas**.
2. Ver tabla con todas las encuestas: tipo, ciclo, total de respuestas y promedio general.
3. El sistema destaca la encuesta con mejor promedio.

---

### 2.6 Auditoría

#### Log de auditoría

Registra todos los cambios en notas e inscripciones (quién cambió qué, valor anterior y nuevo, fecha y hora).

Ir a **Auditoría** → **Log de cambios**.

#### Log de seguridad

Registra todos los intentos de login (exitosos y fallidos).

Ir a **Auditoría** → **Log de seguridad**.

---

## 3. Docente

El docente accede a las materias que tiene asignadas como espacios curriculares.

---

### 3.1 Mis materias

Al iniciar sesión, el panel muestra las materias asignadas al docente en el año lectivo actual, con su curso y comisión correspondientes.

---

### 3.2 Registro de asistencias

1. Ir a **Asistencias** y seleccionar la materia y curso correspondiente.
2. Seleccionar la **fecha** de la clase.
3. El sistema muestra la lista de estudiantes inscriptos en esa materia.
4. Para cada estudiante, marcar: **Presente**, **Ausente Justificado** o **Ausente Injustificado**.
   - Si marcás **Ausente Justificado**, el campo **Motivo** es obligatorio.
5. Hacer clic en **Guardar asistencias**.

> Solo puede haber un registro de asistencia por materia, curso y fecha. Si necesitás corregir un registro ya guardado, usá **Rectificar**.

#### Rectificar asistencia

1. Ir a **Asistencias** → seleccionar la fecha ya registrada.
2. Modificar el estado del estudiante.
3. Guardar.

---

### 3.3 Carga de notas de examen

1. Ir a **Exámenes** y seleccionar el examen correspondiente.
2. Se muestra la lista de estudiantes inscriptos a ese examen.
3. Para cada estudiante, ingresar la nota (entre 1 y 10).
   - Nota ≥ 4 = **Aprobado**.
   - Nota < 4 = **Desaprobado**.
4. Hacer clic en **Guardar notas**.

> Toda carga de nota queda registrada en el log de auditoría con el valor anterior y el nuevo.

#### Rectificar una nota

1. En la lista de inscriptos al examen, hacer clic en **Rectificar** junto a la nota a corregir.
2. Ingresar la nueva nota y el motivo de la rectificación.
3. Guardar.

---

### 3.4 Encuestas de evaluación

El docente puede crear encuestas de evaluación asociadas a sus propias materias y consultar los resultados de forma anónima.

#### Crear una encuesta de evaluación

1. Ir a **Mis encuestas** en el menú lateral.
2. Hacer clic en **Nueva encuesta**.
3. Seleccionar la materia a evaluar y completar título y descripción.
4. Agregar preguntas (Escala Likert o Texto libre).
5. Activar la encuesta cuando esté lista.

> Solo podés crear encuestas para materias que tenés asignadas como espacio curricular.

#### Ver resultados de tus encuestas

1. Ir a **Mis encuestas → Resultados**.
2. Seleccionar la encuesta.
3. Ver: total de respuestas, promedio global, resultado por pregunta y comentarios.

#### Comparativo de tus encuestas

1. Ir a **Mis encuestas → Comparativo**.
2. Ver el promedio de cada encuesta tuya y cuál tuvo mejor puntuación.

---

## 4. Preceptor

El preceptor gestiona las inscripciones y el seguimiento de los estudiantes de su curso.

---

### 4.1 Inscripción de estudiantes a materias

#### Inscribir a un estudiante manualmente

1. Ir a **Inscripciones** → **Nueva inscripción**.
2. Buscar al estudiante por legajo o nombre.
3. Seleccionar la materia y el curso.
4. El sistema valida automáticamente las correlatividades:
   - Si el estudiante no cumple los requisitos, se muestra el motivo y no permite continuar.
5. Confirmar la inscripción.

#### Dar de baja una inscripción

1. En la lista de inscripciones activas, hacer clic en **Dar de baja** junto a la inscripción.
2. Confirmar la acción.

---

### 4.2 Gestión de exámenes

#### Ver inscriptos a un examen

1. Ir a **Exámenes** → seleccionar el examen.
2. Ver la lista de estudiantes inscriptos con su estado.

---

### 4.3 Control de legajo individual

Permite ver el historial completo de un estudiante.

1. Ir a **Reportes** → **Control de legajo**.
2. Ingresar el legajo del estudiante.
3. El sistema muestra:
   - Datos personales y condición académica actual.
   - Materias cursadas con su estado y nota final.
   - Asistencias registradas.
   - Inscripciones activas.

---

### 4.4 Alertas

El preceptor recibe emails automáticos cada lunes con:
- Estudiantes en riesgo de perder la regularidad por inasistencias.
- Estudiantes con inactividad prolongada.
- Recordatorios de fechas límite próximas a vencer.

También puede ejecutar la detección manualmente desde **Alertas** en el menú lateral.

---

## 5. Estudiante

El estudiante puede autogestionar sus inscripciones y consultar su situación académica.

---

### 5.1 Inscripción a materias

1. Ir a **Mis materias** → **Inscribirme**.
2. El sistema muestra las materias disponibles para el año lectivo actual.
3. Las materias para las que el estudiante **no cumple las correlatividades** aparecen deshabilitadas con el motivo (ej. "Requiere tener Matemáticas I regularizada").
4. Seleccionar la materia deseada y confirmar.
5. El sistema genera y muestra un **comprobante de inscripción** que puede descargarse.

> La inscripción solo está habilitada durante el período oficial definido en el Calendario Académico.

---

### 5.2 Encuesta pendiente

Antes de inscribirse a una materia o examen, el sistema puede presentar una encuesta obligatoria que debe completarse primero.

**Cómo funciona:**

1. Al intentar inscribirte, si hay una encuesta activa pendiente, aparece automáticamente un **modal con la encuesta**.
2. Respondé todas las preguntas obligatorias:
   - **Escala Likert:** elegí un valor del 1 (Muy malo) al 5 (Muy bueno).
   - **Texto libre:** escribí tu respuesta en el campo de texto.
3. Hacé clic en **Enviar respuesta**.
4. Una vez enviada, la inscripción continúa normalmente.

> Las respuestas son **anónimas**: el sistema no registra quién respondió qué. Solo se guarda que ya completaste la encuesta para no volverte a molestar con ella.

---

### 5.3 Inscripción a exámenes finales

1. Ir a **Exámenes** → **Inscribirme a un final**.
2. El sistema muestra los exámenes disponibles para materias que el estudiante tiene **regularizadas** (condición necesaria para rendir).
3. Seleccionar el examen y confirmar la inscripción.
4. Se genera un comprobante de inscripción al examen.

> Para rendir un examen final se requiere tener la materia **regularizada**. Para obtener la aprobación definitiva se requiere nota ≥ 4.

---

### 5.3 Mis calificaciones

1. Ir a **Mis calificaciones** o **Mi historial**.
2. Ver todas las notas obtenidas en exámenes rendidos, organizadas por materia y período.

---

### 5.4 Dar de baja una inscripción

Si necesitás cancelar una inscripción a una materia:

1. Ir a **Mis materias** → lista de materias inscriptas.
2. Hacer clic en **Dar de baja** junto a la materia.
3. Confirmar. La baja queda registrada y no podrás reinscribirte en el mismo período.

---

## Notas generales

- **Sesión:** La sesión expira tras un período de inactividad. Si el sistema te pide volver a iniciar sesión, ingresá tus credenciales nuevamente.
- **Soporte:** Para reportar problemas técnicos, contactar al área de administración del instituto.
- **Navegadores compatibles:** Chrome, Firefox, Edge (versiones actuales). No se recomienda Internet Explorer.
