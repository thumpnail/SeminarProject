namespace Chat.Common.Models;

using LiteDB;
using MessagePack;

[MessagePackObject]
public record ChatRoom {
    [Key(0)] [BsonId] public required Identifier Id { get; set; }
    [Key(1)] public required string Name { get; set; } // Raumname
    [Key(2)] public required bool IsPrivate { get; set; } // privat/öffentlich
    [Key(3)] [BsonRef("users")] public required List<User> Users { get; set; }
    [Key(4)] [BsonRef("messages")] public required List<Message> Messages { get; set; }
}