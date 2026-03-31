using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Interfaces;

public interface IJwtService
{
    string GenerarToken(Usuario usuario);
}
