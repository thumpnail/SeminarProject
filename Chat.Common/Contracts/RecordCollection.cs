global using Identifier = string;
global using ID = string;

namespace Chat.Common.Contracts;

using Models;

public record MessageSendContract(string Sender, ID RoomId, string Content, DateTime Sent);
public record MessageSendResponseContract(string Message, bool Success);

public record HistoryRetrieveContract(ID RoomId, DateTime StartDate, int Limit = 50);
public record HistoryResponseContract(List<Message> Messages, bool Success);

public record RoomRetrieveContract(string Sender, string[] Receivers);
public record RoomRetrieveResponseContract(bool Success, string Message = null, ID RoomId = null);