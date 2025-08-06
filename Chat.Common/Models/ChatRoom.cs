namespace Chat.Common.Models;

public class ChatRoom {
    public required Identifier Id { get; set; }
    public required string Name { get; set; } // Raumname
    public required bool IsPrivate { get; set; } // privat/öffentlich
    public required List<Identifier> UserIds { get; set; }
    public required List<Identifier> MessageIds { get; set; }
}