using System.Text.Json;
using System.Text.Json.Serialization;

namespace PracticaProfesional.Infrastructure.Serialization;

/// <summary>
/// SQL Server (datetime2) no guarda si un valor es UTC o local: EF Core siempre devuelve
/// DateTimeKind.Unspecified al leerlo, aunque el dato guardado (DateTime.UtcNow en todo el
/// código) sea UTC. System.Text.Json por defecto no agrega el sufijo "Z" para Unspecified, así
/// que el frontend interpreta la hora como si ya fuera local del navegador — mostrando la hora
/// UTC cruda en vez de restarle el offset (3hs en Argentina). Forzar Kind=Utc acá antes de
/// serializar corrige esto para toda la API de una sola vez.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}

public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
        else
            writer.WriteNullValue();
    }
}
