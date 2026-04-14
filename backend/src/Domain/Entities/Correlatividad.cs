using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Correlatividad
{
    public int Id { get; private set; }
    // Materia que se desea cursar/rendir
    public int MateriaDestinoId { get; private set; }
    public Materia MateriaDestino { get; private set; } = null!;
    // Materia que se debe tener cumplida primero
    public int MateriaRequisitoId { get; private set; }
    public Materia MateriaRequisito { get; private set; } = null!;
    // Cursar o Rendir
    public string TipoRequerimiento { get; private set; } = string.Empty;
    // Regularizado o Aprobado
    public CondicionAcademica CondicionAcademica { get; private set; }

    private Correlatividad() { }

    public static Correlatividad Crear(
        int materiaDestinoId,
        int materiaRequisitoId,
        string tipoRequerimiento,
        CondicionAcademica condicionAcademica)
    {
        if (materiaDestinoId == materiaRequisitoId)
            throw new ArgumentException("La materia destino y requisito no pueden ser la misma.");
        if (string.IsNullOrWhiteSpace(tipoRequerimiento))
            throw new ArgumentException("El tipo de requerimiento es obligatorio.");

        return new Correlatividad
        {
            MateriaDestinoId = materiaDestinoId,
            MateriaRequisitoId = materiaRequisitoId,
            TipoRequerimiento = tipoRequerimiento,
            CondicionAcademica = condicionAcademica
        };
    }
}
