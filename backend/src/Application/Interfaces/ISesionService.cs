namespace PracticaProfesional.Application.Interfaces;

/// <summary>
/// Servicio singleton que mantiene en memoria las sesiones activas.
/// Un usuario es "activo" si envió un heartbeat en los últimos 60 segundos.
/// </summary>
public interface ISesionService
{
    void RegistrarActividad(int usuarioId);
    void RemoverSesion(int usuarioId);
    IEnumerable<int> ObtenerIdsActivos();
}
