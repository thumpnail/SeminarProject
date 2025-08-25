namespace Chat.Common;

using MessagePack;

/// <summary>
/// Repräsentiert ein Login-Token für die Authentifizierung eines Benutzers.
/// </summary>
/// <param name="Username">Benutzername</param>
/// <param name="Token">Authentifizierungs-Token</param>
public record LoginToken(string Username, string Token);