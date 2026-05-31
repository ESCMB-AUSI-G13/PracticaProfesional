namespace PracticaProfesional.Application.Padron.DTOs;

public class ImportarPadronResultDto
{
    public int Total { get; init; }
    public int Cargados { get; init; }
    public int Fallidos { get; init; }
    public List<PadronErrorDto> Errores { get; init; } = [];
}

public class PadronErrorDto
{
    public string DNI { get; init; } = string.Empty;
    public string Motivo { get; init; } = string.Empty;
}
