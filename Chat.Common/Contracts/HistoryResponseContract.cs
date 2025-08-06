using Chat.Common.Models;
using MessagePack;
namespace Chat.Common.Contracts.Contracts;

[MessagePackObject]
public class HistoryResponseContract {
    [Key(0)] public required string UserId { get; init; }
    [Key(1)] public required string ChatId { get; init; }
    [Key(2)] public required List<Message> Messages { get; init; }
}