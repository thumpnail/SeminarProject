using MessagePack;

namespace Chat.Common;

/// <summary>
/// Hilfsklasse für die Serialisierung und Deserialisierung von Objekten mit MessagePack.
/// </summary>
public class MsgPck {
    /// <summary>
    /// Serialisiert ein Objekt in ein Byte-Array.
    /// </summary>
    /// <typeparam name="T">Typ des zu serialisierenden Objekts</typeparam>
    /// <param name="data">Objekt</param>
    /// <returns>Serialisiertes Byte-Array</returns>
    public static byte[] Serialize<T>(T data) {
        return MessagePackSerializer.Serialize(data);
    }
    /// <summary>
    /// Deserialisiert ein Byte-Array in ein Objekt.
    /// </summary>
    /// <typeparam name="T">Zieltyp</typeparam>
    /// <param name="data">Byte-Array</param>
    /// <returns>Deserialisiertes Objekt</returns>
    public static T Deserialize<T>(byte[] data) {
        return MessagePackSerializer.Deserialize<T>(data);
    }
}