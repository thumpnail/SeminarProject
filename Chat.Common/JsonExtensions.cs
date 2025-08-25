namespace Chat.Common;

/// <summary>
/// Erweiterungsmethoden für die JSON-Serialisierung und -Deserialisierung.
/// Nutzt Newtonsoft.Json.
/// </summary>
public static class JsonExtensions {
    /// <summary>
    /// Serialisiert ein Objekt zu einem JSON-String.
    /// </summary>
    /// <typeparam name="T">Typ des Objekts</typeparam>
    /// <param name="obj">Das zu serialisierende Objekt</param>
    /// <returns>JSON-String</returns>
    public static string ToJson<T>(this T obj) {
        return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
    }
    /// <summary>
    /// Deserialisiert einen JSON-String zu einem Objekt.
    /// </summary>
    /// <typeparam name="T">Zieltyp</typeparam>
    /// <param name="json">JSON-String</param>
    /// <returns>Deserialisiertes Objekt</returns>
    public static T? FromJson<T>(this string json) {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }
}