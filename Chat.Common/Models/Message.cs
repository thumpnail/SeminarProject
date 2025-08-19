namespace Chat.Common.Models;

using LiteDB;
using MessagePack;

public record Message {
    [BsonId] public required Identifier Id { get; set; } // 'user1-user2-user3' für Gruppenchats (alphabetisch sortiert), 'user1-user2' für Direktnachrichten
    [BsonRef(CollectionName.USERS)] public required User SendingUser { get; set; } // Sender
    [BsonRef(CollectionName.ROOMS)] public required ChatRoom ChatRoom { get; set; } // Chatraum, in dem die Nachricht gesendet wurde
    public required string Content { get; set; }
    public required DateTime Timestamp { get; set; }
    public static Message GenerateMessage(string sender, string chatRoom, string content) {
        return new Message {
            Id = Guid.NewGuid().ToString(),
            SendingUser = new() {
                Username = sender,
                Id = sender,
                ChatRooms = []
            },
            ChatRoom = default,
            Content = content,
            Timestamp = DateTime.Now
        };
    }
}