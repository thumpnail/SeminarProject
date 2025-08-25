using Chat.Common.Contracts;
using Chat.Common.Models;
namespace Chat.Common;

/// <summary>
/// Schnittstelle für Datenbankoperationen im Chat-System.
/// Implementierungen können verschiedene Backends nutzen (z.B. LiteDB, In-Memory).
/// </summary>
public interface IDatabase {
    /// <summary>
    /// Fügt eine neue Nachricht in die Datenbank ein.
    /// </summary>
    /// <param name="messageSendContract">Daten der zu sendenden Nachricht</param>
    /// <returns>Antwort mit Status und Inhalt</returns>
    public MessageSendResponseContract InsertMessage(MessageSendContract messageSendContract);

    /// <summary>
    /// Holt Nachrichten aus einem Chatraum ab einem bestimmten Zeitpunkt.
    /// </summary>
    /// <param name="historyRetrieveContract">Abfrageparameter</param>
    /// <returns>Antwort mit Nachrichtenliste</returns>
    public HistoryResponseContract GetMessages(HistoryRetrieveContract historyRetrieveContract);

    /// <summary>
    /// Holt oder erstellt Benutzer anhand ihrer Usernamen.
    /// </summary>
    /// <param name="usernames">Liste der Usernamen</param>
    /// <returns>Liste der Benutzerobjekte</returns>
    public List<User> GetOrCreateUsers(List<string> usernames);

    /// <summary>
    /// Holt oder erstellt einen Chatraum anhand der ID.
    /// </summary>
    /// <param name="expectedChatRoomID">ID des Raums</param>
    /// <returns>Chatraum-Objekt</returns>
    public ChatRoom GetOrCreateRoom(string expectedChatRoomID);

    /// <summary>
    /// Aktualisiert einen Chatraum mit einer Benutzerliste.
    /// </summary>
    /// <param name="room">Chatraum</param>
    /// <param name="users">Benutzerliste</param>
    public void UpdateRoomWithUsers(ChatRoom room, List<User> users);

    /// <summary>
    /// Erzeugt eine vergleichbare Raum-ID aus einer Benutzerliste.
    /// </summary>
    /// <param name="userList">Benutzerliste</param>
    /// <returns>Raum-ID</returns>
    public string GetComparableRoomId(List<User> userList);

    /// <summary>
    /// Holt einen Chatraum anhand eines Abfrageobjekts.
    /// </summary>
    /// <param name="roomRetrieveContract">Abfrageparameter</param>
    /// <returns>Antwort mit Chatraumdaten</returns>
    public RoomRetrieveResponseContract GetRoom(RoomRetrieveContract roomRetrieveContract);
}