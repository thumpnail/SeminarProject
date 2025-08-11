namespace Chat.Common.Contracts;
using Models;

public record RegisterUserContract(string Username, string Password);
public record RegisterUserResponseContract(string Message,bool Success,LoginToken LoginToken);

public record MessageSendContract(string Sender, string Receiver, string Content, string roomId);
public record LoginResponseContract(string Message, bool Success, LoginToken LoginToken);

public record MessageSendResponseContract(string Message, bool Success);
public record LoginContract(string Username, string Password);

public record HistoryRetrieveContract(string UserId, string ChatId, int Limit, DateTime StartDate);
public record HistoryResponseContract(string UserId, string ChatId,List<Message> Messages);

public record CreateRoomResponseContract();
public record CreateRoomContract(string RoomName, string Description, string CreatorUsername, ChatRoom ChatRoom);

public record AddUserToRoomResponseContract(string Message, bool Success, Identifier RoomId, Identifier UserId);
public record AddUserToRoomContract(Identifier RoomId, Identifier UserId);
