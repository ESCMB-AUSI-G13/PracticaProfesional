using System.Text.RegularExpressions;
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
        ValidarFormatoDni(dni);
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio.");
        ValidarFormatoEmail(email);
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
        => string.Equals(PasswordResetToken, token?.Trim(), StringComparison.OrdinalIgnoreCase)
           && PasswordResetTokenExpiry > DateTime.UtcNow;

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
        ValidarFormatoEmail(email);

        Nombre = nombre;
        Apellido = apellido;
        Email = email.ToLowerInvariant();
        Rol = rol;
    }

    // Antes solo se validaba unicidad (DNI/Email ya en uso) pero no el formato — el backend
    // aceptaba un DNI como "ABC123XYZ" o un email como "no-es-un-email" (`201 Created`), y
    // solo el frontend los rechazaba con Validators.pattern/Validators.email. Cualquier request
    // directo a la API bypaseaba esas reglas. Mismo criterio de formato que PadronAlumno.Crear
    // (ver CHECKLIST.md, Tier 5 #16).
    private static readonly Regex FormatoEmail =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private static void ValidarFormatoDni(string dni)
    {
        var limpio = dni.Trim();
        if (!limpio.All(char.IsDigit))
            throw new ArgumentException("El DNI solo puede contener dígitos.");
        if (limpio.Length < 7 || limpio.Length > 10)
            throw new ArgumentException("El DNI debe tener entre 7 y 10 dígitos.");
    }

    private static void ValidarFormatoEmail(string email)
    {
        if (!FormatoEmail.IsMatch(email.Trim()))
            throw new ArgumentException("El formato del email no es válido.");
    }

    // Antes cada UseCase de alta duplicaba "Length < 6" a mano (y RestablecerPasswordUseCase no
    // validaba nada) — el frontend exige 8+ caracteres pero el backend aceptaba 6, bypasseable
    // con requests directos. Un único lugar para el mínimo evita que quede desalineado de nuevo
    // (ver CHECKLIST.md, Tier 7 #33/#37).
    public const int LongitudMinimaPassword = 8;

    public static void ValidarFortalezaPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < LongitudMinimaPassword)
            throw new ArgumentException($"La clave debe tener al menos {LongitudMinimaPassword} caracteres.");
    }
}
