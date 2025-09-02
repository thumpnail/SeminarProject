using System.Collections.Concurrent;
using Chat.Common.Contracts;
using Chat.Common.Models;
namespace Chat.Common;

/// <summary>
/// In-Memory-Datenbank für das Chat-System.
/// Ideal für Tests und lokale Entwicklung.
/// </summary>
public class LocalDatabase : IDatabase {
    private readonly ConcurrentDictionary<string, User> userDictionary = new();
    private readonly ConcurrentDictionary<string, ChatRoom> roomDictionary = new();
    private readonly List<Message> messageList = new();

    /// <summary>
    /// Initialisiert die In-Memory-Datenbank.
    /// </summary>
    /// <param name="connectionString">Verbindungszeichenfolge (optional)</param>
    public LocalDatabase(string connectionString = "in-memory" ) {
        // In-memory database, no initialization needed
    }

    /// <inheritdoc/>
    public MessageSendResponseContract InsertMessage(MessageSendContract messageSendContract) {
        var user = userDictionary.GetValueOrDefault(messageSendContract.Sender);
        if (user == null) {
            return new MessageSendResponseContract("Sender not found", false, new());
        }
        if (messageSendContract.RoomId is null) {
            return new MessageSendResponseContract("Room was null", false, new());
        }
        var room = roomDictionary.TryGetValue(messageSendContract.RoomId, out var foundRoom) ? foundRoom : null;
        if (room == null) {
            return new MessageSendResponseContract("Room not found", false, new());
        }

        var message = new Message {
            Id = Guid.NewGuid().ToString(),
            SendingUser = user,
            ChatRoom = room,
            Content = messageSendContract.Content,
            Timestamp = DateTime.Now
        };

        messageList.Add(message);
        room.Messages.Add(message);

        return new MessageSendResponseContract(message.Content, true, new());
    }

    /// <inheritdoc/>
    public HistoryResponseContract GetMessages(HistoryRetrieveContract historyRetrieveContract) {
        var messages = messageList
            .Where(m => m.ChatRoom.Id == historyRetrieveContract.RoomId)
            .Where(m=> m.Timestamp > historyRetrieveContract.StartDate)
            .ToList();

        return new HistoryResponseContract(messages, true, new());
    }

    /// <inheritdoc/>
    public List<User> GetOrCreateUsers(List<string> usernames) {
        var users = new List<User>();
        foreach (var username in usernames) {
            if (!userDictionary.TryGetValue(username, out var user)) {
                user = new User {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    ChatRooms = new List<ChatRoom>()
                };
                userDictionary[username] = user;
            }
            users.Add(user);
        }
        return users;
    }
    /// <inheritdoc/>
    public ChatRoom GetOrCreateRoom(string expectedChatRoomID) {
        if (!roomDictionary.TryGetValue(expectedChatRoomID, out var room)) {
            room = new ChatRoom {
                Id = expectedChatRoomID,
                ComparableUserBasedId = expectedChatRoomID,
                Users = new List<User>(),
                Messages = new List<Message>()
            };
            roomDictionary[expectedChatRoomID] = room;
        }
        return room;
    }
    /// <inheritdoc/>
    public void UpdateRoomWithUsers(ChatRoom room, List<User> users) {
        foreach (var user in users) {
            if (room.Users.All(u => u.Id != user.Id)) {
                room.Users.Add(user);
            }
            if (user.ChatRooms.All(r => r.Id != room.Id)) {
                user.ChatRooms.Add(room);
            }
        }
    }
    /// <inheritdoc/>
    public string GetComparableRoomId(List<User> userList) {
        return string.Join("-", userList.Select(u => u.Id).OrderBy(id => id));
    }
    /// <inheritdoc/>
    public RoomRetrieveResponseContract GetRoom(RoomRetrieveContract roomRetrieveContract) {
        var usernames = new List<string> { roomRetrieveContract.Sender };
        usernames.AddRange(roomRetrieveContract.Receivers);

        var users = GetOrCreateUsers(usernames);
        var expectedChatRoomID = GetComparableRoomId(users);
        var room = GetOrCreateRoom(expectedChatRoomID);

        UpdateRoomWithUsers(room, users);

        return new RoomRetrieveResponseContract(true, String.Empty, RoomId: room.Id, new());
    }
}