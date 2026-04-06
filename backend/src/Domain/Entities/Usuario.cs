using PracticaProfesional.Domain.Enums;

namespace PracticaProfesional.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string DNI { get; private set; } = string.Empty;
    public string Legajo { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Rol Rol { get; private set; }
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    // Reset de contraseña
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // Constructor para EF Core
    private Usuario() { }

    public static Usuario Crear(
        string dni,
        string legajo,
        string email,
        string nombre,
        string apellido,
        string passwordHash,
        Rol rol)
    {
        if (string.IsNullOrWhiteSpace(dni)) throw new ArgumentException("El DNI es obligatorio.");
        if (string.IsNullOrWhiteSpace(legajo)) throw new ArgumentException("El legajo es obligatorio.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio.");
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(apellido)) throw new ArgumentException("El apellido es obligatorio.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("La contraseña es obligatoria.");

        return new Usuario
        {
            DNI = dni,
            Legajo = legajo,
            Email = email.ToLowerInvariant(),
            Nombre = nombre,
            Apellido = apellido,
            PasswordHash = passwordHash,
            Rol = rol,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };
    }

    public bool VerificarPassword(string passwordPlana)
        => BCrypt.Net.BCrypt.Verify(passwordPlana, PasswordHash);

    public void Desactivar() => Activo = false;

    public void Reactivar() => Activo = true;

    public void GenerarTokenReset()
    {
        PasswordResetToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
    }

    public bool EsTokenResetValido(string token)
        => PasswordResetToken == token && PasswordResetTokenExpiry > DateTime.UtcNow;

    public void RestablecerPassword(string nuevoPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoPasswordHash)) throw new ArgumentException("La contraseña es obligatoria.");
        PasswordHash = nuevoPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }

    public void Modificar(string nombre, string apellido, string email, Rol rol)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(apellido)) throw new ArgumentException("El apellido es obligatorio.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio.");

        Nombre = nombre;
        Apellido = apellido;
        Email = email.ToLowerInvariant();
        Rol = rol;
    }
}
