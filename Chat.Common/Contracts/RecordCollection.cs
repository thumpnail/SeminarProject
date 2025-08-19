namespace Chat.Common.Contracts;
using Models;

public record MessageSendContract(string Sender, string Receiver, string Content);
public record MessageSendResponseContract(string Message, bool Success);

public record HistoryRetrieveContract(Identifier SenderId, Identifier ReceiverId, DateTime StartDate, int Limit = 50);
public record HistoryResponseContract(List<Message> Messages, bool Success);