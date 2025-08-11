namespace Chat.Common.Models;

using LiteDB;
using MessagePack;

[MessagePackObject]
public record Token {
	[Key(0)] [BsonId] public required string Id { get; init; }
	[Key(1)] public required string Username { get; init; }
	[Key(2)] public required string TokenValue { get; init; }
	[Key(3)] public required DateTime Creation { get; init; }
	[Key(4)] public required DateTime Expiration { get; init; }
	[Key(5)] public required string RefreshToken { get; init; }
	[Key(6)] public required bool IsRefreshToken { get; init; }
	[Key(7)] [BsonRef("users")] public required User User { get; init; }
}
