namespace Chat.Common;

using MessagePack;

public record LoginToken(string Username, string Token);