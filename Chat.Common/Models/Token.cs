namespace Chat.Common.Models;

using LiteDB;
using MessagePack;

public record Token {
	[BsonId] public required string Id { get; init; }
	public required string Username { get; init; }
	public required string TokenValue { get; init; }
	public required DateTime Creation { get; init; }
	public required DateTime Expiration { get; init; }
	public required string RefreshToken { get; init; }
	public required bool IsRefreshToken { get; init; }
	[BsonRef(CollectionName.USERS)] public required User Owner { get; init; }
}
