using PracticaProfesional.Application.Interfaces;
using PracticaProfesional.Application.Padron.DTOs;
using PracticaProfesional.Domain.Entities;

namespace PracticaProfesional.Application.Padron;

public class CargarPadronUseCase(IPadronRepository padronRepository)
{
    public async Task<ImportarPadronResultDto> EjecutarAsync(
        IEnumerable<string> dnis,
        CancellationToken cancellationToken = default)
    {
        var lista = dnis.ToList();
        var errores = new List<PadronErrorDto>();
        var cargados = 0;
        var vistos = new HashSet<string>();

        foreach (var dniRaw in lista)
        {
            var dni = dniRaw?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(dni))
            {
                errores.Add(new PadronErrorDto { DNI = "(vacío)", Motivo = "El DNI está vacío." });
                continue;
            }

            if (vistos.Contains(dni))
            {
                errores.Add(new PadronErrorDto { DNI = dni, Motivo = "DNI duplicado en el archivo." });
                continue;
            }

            vistos.Add(dni);

            PadronAlumno padron;
            try
            {
                padron = PadronAlumno.Crear(dni);
            }
            catch (ArgumentException ex)
            {
                errores.Add(new PadronErrorDto { DNI = dni, Motivo = ex.Message });
                continue;
            }

            if (await padronRepository.ExisteDniAsync(dni, cancellationToken))
            {
                errores.Add(new PadronErrorDto { DNI = dni, Motivo = "Ya existe en el padrón." });
                continue;
            }

            await padronRepository.AgregarAsync(padron, cancellationToken);
            cargados++;
        }

        return new ImportarPadronResultDto
        {
            Total = lista.Count,
            Cargados = cargados,
            Fallidos = errores.Count,
            Errores = errores
        };
    }
}
