using Chat.Common.Models;

using LiteDB;

using Microsoft.Extensions.Hosting;
namespace ChatApp.Server.Services;

public class DatabaseService {
    public const string DATABASE_PATH = "./chat.db";
    public LiteDatabase db = new(DATABASE_PATH);
    public ILiteCollection<ChatRoom> roomCollection { get; set; }
    public ILiteCollection<User> usersCollection { get; set; }
    public ILiteCollection<Message> messagesCollection { get; set; }

    public DatabaseService() {
        roomCollection = db.GetCollection<ChatRoom>(CollectionName.ROOMS);
        usersCollection = db.GetCollection<User>(CollectionName.USERS);
        messagesCollection = db.GetCollection<Message>(CollectionName.MESSAGES);
    }

    public List<User> GetOrCreateUsers(List<string> usernames) {
        var users = new List<User>();
        foreach (var username in usernames) {
            User refUser;
            refUser = usersCollection.FindOne(u => u.Username.Equals(username));
            // if the user does not exist, create a new user
            if (refUser is null) {
                var newUser = new User {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    ChatRooms = []
                };
                usersCollection.Insert(newUser);
                users.Add(newUser);
            } else {
                // add the existing user to the userList
                users.Add(refUser);
            }
        }
        return users;
    }

    public ChatRoom GetOrCreateRoom(string expectedChatRoomID) {
        // Check if the room already exists
        ChatRoom room;
        room = roomCollection.FindOne(r => r.Id.Equals(expectedChatRoomID));
        // If the room does not exist, create a new one
        if (room is null) {
            room = new() {
                Id = expectedChatRoomID,
                ComparableUserBasedId = expectedChatRoomID,
                Users = new List<User>()
            };
            roomCollection.Insert(room);
        }
        return room;
    }

    public void UpdateRoomWithUsers(ChatRoom room, List<User> users) {
        // Füge nur Benutzer hinzu, die noch nicht im Raum sind
        room.Users = users.Where(u => room.Users.All(existing => existing.Id != u.Id)).Concat(room.Users).ToList();
        // Aktualisiere den Raum in der Datenbank
        roomCollection.Update(room);
        // update users with the room
        foreach (var user in users.Where(user => !user.ChatRooms.Contains(room))) {
            user.ChatRooms.Add(room);
        }
        // Update the users in the database
        usersCollection.Update(users);
    }

    public string GetComparableRoomId(List<User> userList) => string.Join("-", userList.Select(r => r.Id).OrderBy(r => r));
}