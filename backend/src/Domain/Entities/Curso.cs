using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Curso
{
    public int Id { get; private set; }

    // OJO — nomenclatura no intuitiva pero consistente en todo el sistema (DTOs, repositorios,
    // reportes): "Anio" es el año CALENDARIO (ej. 2026), "AnioLectivo" es el año del PLAN de
    // estudios/cursada (1 a 6). Uno esperaría lo inverso por los nombres. No se renombra acá
    // porque tocaría entidad + migraciones + DTOs + frontend en todo el sistema para un problema
    // que es solo de claridad, no funcional — se documenta explícitamente en su lugar
    // (ver CHECKLIST.md, Tier 7 #31).
    /// <summary>Año calendario del curso (ej. 2026) — NO es el año de cursada del plan.</summary>
    public int Anio { get; private set; }
    /// <summary>Año de cursada dentro del plan de estudios (1 a 6) — pese al nombre, NO es el año calendario.</summary>
    public int AnioLectivo { get; private set; }
    public string Comision { get; private set; } = string.Empty;
    public int Cupo { get; private set; }
    public EstadoCurso Estado { get; private set; }
    public int PreceptorId { get; private set; }
    public int CarreraId { get; private set; }
    public Preceptor Preceptor { get; private set; } = null!;

    private Curso() { }

    /// <param name="anio">Año CALENDARIO (ej. 2026) — no confundir con el año del plan.</param>
    /// <param name="anioLectivo">Año de CURSADA dentro del plan de estudios (1 a 6).</param>
    public static Curso Crear(int anio, int anioLectivo, string comision, int cupo, int preceptorId, int carreraId)
    {
        if (anio < 2000 || anio > 2100) throw new ArgumentException($"El año calendario ({anio}) no es válido — debe estar entre 2000 y 2100.");
        if (anioLectivo < 1 || anioLectivo > 6) throw new ArgumentException($"El año de cursada ({anioLectivo}) debe estar entre 1 y 6.");
        if (string.IsNullOrWhiteSpace(comision)) throw new ArgumentException("La comisión es obligatoria.");
        if (cupo <= 0) throw new ArgumentException("El cupo debe ser mayor a cero.");
        if (carreraId <= 0) throw new ArgumentException("La carrera es obligatoria.");

        return new Curso
        {
            Anio = anio,
            AnioLectivo = anioLectivo,
            Comision = comision.ToUpperInvariant(),
            Cupo = cupo,
            Estado = EstadoCurso.Activo,
            PreceptorId = preceptorId,
            CarreraId = carreraId
        };
    }

    public void Cerrar()
    {
        if (Estado == EstadoCurso.Cerrado)
            throw new PracticaProfesional.Domain.Exceptions.BusinessException("El curso ya está cerrado.", 409);
        Estado = EstadoCurso.Cerrado;
    }
    public void Suspender() => Estado = EstadoCurso.Suspendido;
    public void Reactivar() => Estado = EstadoCurso.Activo;

    public void Modificar(string comision, int cupo, int preceptorId)
    {
        if (string.IsNullOrWhiteSpace(comision)) throw new ArgumentException("La comisión es obligatoria.");
        if (cupo <= 0) throw new ArgumentException("El cupo debe ser mayor a cero.");
        if (preceptorId <= 0) throw new ArgumentException("El preceptor es obligatorio.");
        Comision = comision.ToUpperInvariant();
        Cupo = cupo;
        PreceptorId = preceptorId;
    }
}
