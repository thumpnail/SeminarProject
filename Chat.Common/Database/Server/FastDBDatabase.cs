using Chat.Common.Contracts;
using Chat.Common.Models;

using LiteDB;

using Stellar.Collections;
namespace Chat.Common;

/// <summary>
/// Datenbank-Implementierung auf Basis von LiteDB.
/// Persistiert Nachrichten, Benutzer und Räume lokal als Datei.
/// </summary>
public class FastDBDatabase : IDatabase {
    private FastDB db;
    private IFastDBCollection<Guid,ChatRoom> roomCollection;
    private IFastDBCollection<Guid,User> usersCollection;
    private IFastDBCollection<Guid,Message> messagesCollection;

    /// <summary>
    /// Initialisiert die LiteDB-Datenbank und Collections.
    /// </summary>
    /// <param name="databasePath">Pfad zur Datenbankdatei</param>
    public FastDBDatabase(string databasePath = "../../../../../shared-chat.db") {
        db = new(databasePath);
        roomCollection = db.GetCollection<Guid,ChatRoom>(CollectionName.ROOMS);
        usersCollection = db.GetCollection<Guid,User>(CollectionName.USERS);
        messagesCollection = db.GetCollection<Guid,Message>(CollectionName.MESSAGES);
    }

    /// <inheritdoc/>
    public MessageSendResponseContract InsertMessage(MessageSendContract messageSendContract) {
        var user = usersCollection.Single(x=>x.Username.Equals(messageSendContract.Sender));
        // get room reference
        var room = roomCollection.Single(r => r.Id == messageSendContract.RoomId);
        // add Message to the room
        var message = new Message {
            Id = Guid.NewGuid().ToString(),
            SendingUser = user,
            ChatRoom = room,
            Content = messageSendContract.Content,
            Timestamp = DateTime.Now
        };
        messagesCollection.Add(Guid.Parse(message.Id), message);
        room.Messages.Add(message);

        roomCollection.Update(Guid.Parse(room.Id), room);
        messagesCollection.Update(Guid.Parse(message.Id), message);

        return new MessageSendResponseContract(messageSendContract.runIndexIdentifier, message.Content, true, new());
    }

    /// <inheritdoc/>
    public HistoryResponseContract GetMessages(HistoryRetrieveContract historyRetrieveContract) {
        var room = roomCollection.Single(r => r.Id.Equals(historyRetrieveContract.RoomId));

        var messages = messagesCollection
            .Where(x => x.ChatRoom.Id.Equals(historyRetrieveContract.RoomId))
            .Where(x => x.Timestamp > historyRetrieveContract.StartDate)
            .ToList();
        return new HistoryResponseContract(historyRetrieveContract.runIndexIdentifier, messages, true, new());
    }
    /// <inheritdoc/>
    public List<User> GetOrCreateUsers(params List<string> usernames) {
        var users = new List<User>();
        foreach (var username in usernames) {
            var refUser = usersCollection.SingleOrDefault(u => u.Username.Equals(username));
            // if the user does not exist, create a new user
            if (refUser is null) {
                var newUser = new User {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    ChatRooms = []
                };
                usersCollection.Add(Guid.Parse(newUser.Id), newUser);
                users.Add(newUser);
            } else {
                // add the existing user to the userList
                users.Add(refUser);
            }
        }
        return users;
    }
    /// <inheritdoc/>
    public ChatRoom GetOrCreateRoom(string expectedChatRoomID) {
        // Check if the room already exists
        var room = roomCollection.SingleOrDefault(r => r.Id.Equals(expectedChatRoomID));
        // If the room does not exist, create a new one
        if (room is null) {
            room = new() {
                Id = expectedChatRoomID,
                ComparableUserBasedId = expectedChatRoomID,
                Users = new List<User>()
            };
            roomCollection.Add(Guid.Parse(room.Id), room);
        }
        return room;
    }
    /// <inheritdoc/>
    public void UpdateRoomWithUsers(ChatRoom room, List<User> users) {
        // Füge nur Benutzer hinzu, die noch nicht im Raum sind
        room.Users = users.Where(u => room.Users.All(existing => existing.Id != u.Id)).Concat(room.Users).ToList();
        // Aktualisiere den Raum in der Datenbank
        roomCollection.Update(Guid.Parse(room.Id), room);
        // update users with the room
        foreach (var user in users.Where(user => !user.ChatRooms.Contains(room))) {
            user.ChatRooms.Add(room);
            usersCollection.Update(Guid.Parse(user.Id), user);
        }
        // Update the users in the database
        //.Update(users);
    }
    /// <inheritdoc/>
    public string GetComparableRoomId(List<User> userList) => string.Join("-", userList.Select(r => r.Id).OrderBy(r => r));
    /// <inheritdoc/>
    public RoomRetrieveResponseContract GetRoom(RoomRetrieveContract roomRetrieveContract) {
        List<string> usernames = [roomRetrieveContract.Sender];
        usernames.AddRange(roomRetrieveContract.Receivers);

        var users = GetOrCreateUsers(usernames);

        var expectedChatRoomID = GetComparableRoomId(users);

        // ERROR: creates allways a new room
        var room = GetOrCreateRoom(expectedChatRoomID);

        UpdateRoomWithUsers(room, users);

        return new RoomRetrieveResponseContract(roomRetrieveContract.runIndexIdentifier, true, string.Empty, RoomId: room.Id, new());
    }
}