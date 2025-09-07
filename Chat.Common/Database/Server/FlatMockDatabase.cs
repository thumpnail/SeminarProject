using Chat.Common.Contracts;
using Chat.Common.Models;
namespace Chat.Common;

public class FlatMockDatabase : IDatabase {

    public FlatMockDatabase(string databasePath) {
        // No actual database connection is made in the mock
    }
    public MessageSendResponseContract InsertMessage(MessageSendContract messageSendContract) {
        Thread.Sleep(5);
        return new MessageSendResponseContract(messageSendContract.runIndexIdentifier,"A Message", true, new());
    }
    public HistoryResponseContract GetMessages(HistoryRetrieveContract historyRetrieveContract) {
        Thread.Sleep(5);
        return new HistoryResponseContract(historyRetrieveContract.runIndexIdentifier, [], true, new());
    }
    public List<User> GetOrCreateUsers(List<string> usernames) {
        Thread.Sleep(5);
        return [new User {
                Id = "1",
                Username = "User1",
                ChatRooms = [
                    new ChatRoom {
                        Id = "1",
                        ComparableUserBasedId = "User1-User2",
                        Users = null,
                        Messages = null
                    }
                ]
            }
        ];
    }
    public ChatRoom GetOrCreateRoom(string expectedChatRoomID) {
        Thread.Sleep(5);
        return new ChatRoom {
            Id = "1",
            ComparableUserBasedId = "User1-User2",
            Users = null,
            Messages = null
        };
    }
    public void UpdateRoomWithUsers(ChatRoom room, List<User> users) {
        Thread.Sleep(5);
    }
    public string GetComparableRoomId(List<User> userList) {
        Thread.Sleep(5);
        return "User1-User2";
    }
    public RoomRetrieveResponseContract GetRoom(RoomRetrieveContract roomRetrieveContract) {
        Thread.Sleep(5);
        return new RoomRetrieveResponseContract(roomRetrieveContract.runIndexIdentifier, true, "Room Retrieved", "1", new());
    }
}