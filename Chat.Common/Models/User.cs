namespace Chat.Common.Models;

using LiteDB;
using MessagePack;

[MessagePackObject]
public record User {
    [Key(0)] [BsonId] public required Identifier Id { get; set; }
    [Key(1)] public required string Username { get; set; }
    // todo: This is cleartext, should be hashed in production
    [Key(3)] public required string Password { get; set; }
    [Key(4)] [BsonRef("chats")] public required List<ChatRoom> ChatRooms { get; set; }
}