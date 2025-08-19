namespace Chat.Common.Models;
using LiteDB;
using MessagePack;

public record User {
    [BsonId] public required Identifier Id { get; set; }
    public required string Username { get; set; }

    [BsonRef(CollectionName.ROOMS)]
    public required List<ChatRoom> ChatRooms { get; set; }
}