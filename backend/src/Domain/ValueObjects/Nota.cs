using PracticaProfesional.Domain.Exceptions;

namespace PracticaProfesional.Domain.ValueObjects;

public sealed class Nota
{
    public decimal Valor { get; }
    public bool EsAprobado => Valor >= 4;

    private Nota(decimal valor) => Valor = valor;

    public static Nota Crear(decimal valor)
    {
        if (valor < 1 || valor > 10)
            throw new BusinessException("La nota debe estar entre 1 y 10.");
        return new Nota(Math.Round(valor, 2));
    }

    public override string ToString() => Valor.ToString("F2");
    public override bool Equals(object? obj) => obj is Nota n && n.Valor == Valor;
    public override int GetHashCode() => Valor.GetHashCode();
}
