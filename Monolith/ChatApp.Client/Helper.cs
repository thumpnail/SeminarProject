using System.Net.Http.Json;

using Chat.Common.Contracts;
namespace ChatApp.Client;

public static class Helper {
    public static DateTime lastMessageTimestamp = DateTime.MinValue;
    public static async Task<MessageSendResponseContract> SendMessageAsync(this HttpClient client, MessageSendContract messageContract) {
        var response = await client.PostAsJsonAsync("/send", messageContract);
        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<MessageSendResponseContract>();
        }
        if (response.StatusCode == System.Net.HttpStatusCode.RequestTimeout) {
            Console.WriteLine("Request timed out. Please try again later.");
            return new MessageSendResponseContract("Request timed out.", false);
        }
        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) {
            Console.WriteLine("Internal server error. Please try again later.");
            return new MessageSendResponseContract("Internal server error.", false);
        }
        Console.WriteLine("System Failed to send Message.");
        return new MessageSendResponseContract("Failed to send Message.", false);
    }
// This method retrieves the welcome Message from the chat service
    public static void GetWelcomeMessage(this HttpClient httpClient) {
        try {
            var welcomeMsg = httpClient.GetAsync("/").Result;
            Console.WriteLine(welcomeMsg.Content.ReadAsStringAsync().Result);
        } catch (Exception e) {
            Console.WriteLine("Error connecting to the chat service");
        }
    }
    public static async Task<string> GetRoomAsync(this HttpClient client, string sender, string[] receivers) {
        var roomResponse = await client.PostAsJsonAsync("/getroom",
        new RoomRetrieveContract(sender, receivers));

        if (roomResponse.IsSuccessStatusCode) {
            var room = await roomResponse.Content.ReadFromJsonAsync<RoomRetrieveResponseContract>();
            if (room is { RoomId: not null }) {
                Console.WriteLine($"Room ID: {room.RoomId}");
                return room.RoomId;
            }
        }
        Console.WriteLine("Failed to retrieve room information.");
        return null;
    }
    public static async Task FetchLastMessages(this HttpClient client, string roomId) {
        var historyRetrieveContract = new HistoryRetrieveContract(roomId, lastMessageTimestamp, -1);
        lastMessageTimestamp = DateTime.Now;
        var historyResponse = await client.PostAsJsonAsync("/getMessages", historyRetrieveContract);
        if (historyResponse.IsSuccessStatusCode) {
            var history = await historyResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();
            if (history != null && history.Messages.Count > 0) {
                foreach (var message in history.Messages) {
                    Console.WriteLine($"{message.Content}");
                }
            }
        }
    }

    public static async Task GetChatHistory(this HttpClient client, string roomId) {
        // Fetch the last messages from the chat history
        lastMessageTimestamp = DateTime.Now.AddDays(-1);
        // Retrieve the chat history for the room
        var historyResponse = await client.PostAsJsonAsync("/getMessages", new HistoryRetrieveContract(roomId, lastMessageTimestamp, 50));
        if (historyResponse.IsSuccessStatusCode) {
            // Read the response content as HistoryResponseContract
            var history = await historyResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();
            if (history != null && history.Messages.Count > 0) {
                Console.WriteLine("Chat History:");
                foreach (var message in history?.Messages) {
                    lastMessageTimestamp = message.Timestamp;
                    Console.WriteLine($"{message?.SendingUser?.Username}: {message?.Content}");
                }
            } else {
                Console.WriteLine("No chat history found.");
            }
        } else {
            Console.WriteLine("Failed to retrieve chat history.");
        }
    }
}