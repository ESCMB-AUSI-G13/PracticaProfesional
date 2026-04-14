namespace PracticaProfesional.Domain.Entities;

public class EspacioCurricular
{
    public int Id { get; private set; }
    public int MateriaId { get; private set; }
    public Materia Materia { get; private set; } = null!;
    public int DocenteId { get; private set; }
    public Docente Docente { get; private set; } = null!;
    public int CursoId { get; private set; }
    public Curso Curso { get; private set; } = null!;

    private EspacioCurricular() { }

    public static EspacioCurricular Crear(int materiaId, int docenteId, int cursoId)
    {
        return new EspacioCurricular
        {
            MateriaId = materiaId,
            DocenteId = docenteId,
            CursoId = cursoId
        };
    }
}
