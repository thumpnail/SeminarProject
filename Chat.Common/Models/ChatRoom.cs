namespace Chat.Common.Models;

using LiteDB;
using MessagePack;

public record ChatRoom {
    [BsonId] public required Identifier Id { get; set; }
    [BsonRef(CollectionName.USERS)] public required List<User> Users { get; set; }
    [BsonRef(CollectionName.MESSAGES)] public required List<Message> Messages { get; set; }
    public readonly DateTime CreationDate = DateTime.Now;
}