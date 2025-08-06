using MessagePack;
namespace Chat.Common.Contracts;

[MessagePackObject]
public class HistoryRetrieveContract {
    [Key(0)] public required string UserId { get; init; }
    [Key(1)] public required string ChatId { get; init; }
    [Key(2)] public required int Limit { get; init; } = 50; // Default limit for history retrieval
    [Key(3)] public required DateTime StartDate { get; init; } // Default start date for history retrieval
}