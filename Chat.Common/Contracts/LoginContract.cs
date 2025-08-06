using MessagePack;
namespace Chat.Common.Contracts;

[MessagePackObject]
public class LoginContract {
    [Key(0)] public required string Username { get; set; } = String.Empty;
    [Key(1)] public required string Password { get; set; } = String.Empty;
}