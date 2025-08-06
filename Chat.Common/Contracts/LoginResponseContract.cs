using MessagePack;
namespace Chat.Common.Contracts;

[MessagePackObject]
public class LoginResponseContract {
    [Key(0)] public required string Message { get; init; }
    [Key(1)] public required bool Success { get; init; }
}