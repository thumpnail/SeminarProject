global using Identifier = string;
global using ID = string;

namespace Chat.Common.Contracts;

using Models;

public record MessageSendContract(string Sender, ID RoomId, string Content, DateTime Sent);
public record MessageSendResponseContract(string Message, bool Success);

/// <summary>
///
/// </summary>
/// <param name="RoomId">the room in question</param>
/// <param name="StartDate">the date wich defines the date from when the messages get pulled</param>
/// <param name="Limit">is -1 when no limit is needed since we need to ensure that all messages get transmitted between checks</param>
public record HistoryRetrieveContract(ID RoomId, DateTime StartDate, int Limit = 50);
public record HistoryResponseContract(List<Message> Messages, bool Success);

public record RoomRetrieveContract(string Sender, string[] Receivers);
public record RoomRetrieveResponseContract(bool Success, string Message = null, ID RoomId = null);