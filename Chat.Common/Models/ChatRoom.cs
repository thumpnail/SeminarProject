namespace Chat.Common.Models;

using LiteDB;
using MessagePack;

public record ChatRoom {
    [BsonId] public required Identifier Id { get; set; }
    public required string ComparableUserBasedId { get; set; }
    [BsonRef(CollectionName.USERS)] public List<User> Users { get; set; } = new();
    [BsonRef(CollectionName.MESSAGES)] public List<Message> Messages { get; set; } = new();
    public readonly DateTime CreationDate = DateTime.Now;
}