using System.Runtime.Intrinsics.Arm;
using MessagePack;
namespace Chat.Common.Contracts;

[MessagePackObject]
public class MessageSendContract {
    [Key(0)] public required string Sender { get; init; }
    [Key(1)] public required string Receiver { get; init; }
    [Key(2)] public required string Content { get; init; }
}