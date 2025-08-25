namespace Chat.Common;

/// <summary>
/// Kontextinformationen zu einem Benutzer im Chat-System.
/// Wird für Authentifizierung und Identifikation verwendet.
/// </summary>
public class UserContext {
	/// <summary>
	/// Eindeutige Benutzer-ID.
	/// </summary>
	public required string UserId { get; set; }
	/// <summary>
	/// Benutzername.
	/// </summary>
	public required string Username { get; set; }
}
