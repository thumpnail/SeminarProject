namespace Chat.Common.Models;

public class Message {
    public required Identifier ID { get; set; }
    public required Identifier UserId { get; set; } // Sender
    public required Identifier ReceiverId { get; set; } // Empfänger (optional für Direktnachrichten)
    public required Identifier ChatRoomId { get; set; } // Referenz auf den Raum
    public required string Content { get; set; }
    public required DateTime Timestamp { get; set; }
}