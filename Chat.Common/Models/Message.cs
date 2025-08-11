namespace Chat.Common.Models;

using LiteDB;
using MessagePack;

[MessagePackObject]
public record Message {
    [Key(0)] [BsonId] public required Identifier Id { get; set; }
    [Key(1)] [BsonRef("users")] public required User User { get; set; } // Sender
    [Key(2)] [BsonRef("users")] public required User ReceivingUser { get; set; } // Empfänger (optional für Direktnachrichten)
    [Key(3)] [BsonRef("chats")] public required ChatRoom ChatRoom { get; set; } // Referenz auf den Raum
    [Key(4)] public required string Content { get; set; }
    [Key(5)] public required DateTime Timestamp { get; set; }
}