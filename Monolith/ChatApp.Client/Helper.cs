using System.Net.Http.Json;

using Chat.Common.Contracts;
using Chat.Common.Models;
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
            return new MessageSendResponseContract(messageContract.runIndexIdentifier, "Request timed out.", false, new BenchmarkTag());
        }
        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) {
            Console.WriteLine("Internal server error. Please try again later.");
            return new MessageSendResponseContract(messageContract.runIndexIdentifier, "Internal server error.", false, new BenchmarkTag());
        }
        Console.WriteLine("System Failed to send Message.");
        return new MessageSendResponseContract(messageContract.runIndexIdentifier, "Failed to send Message.", false, new BenchmarkTag());
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
    public static async Task<RoomRetrieveResponseContract> GetRoomAsync(this HttpClient client, string runIndexIdentifier, string sender, string[] receivers) {
        var roomResponse = await client.PostAsJsonAsync("/room",
            new RoomRetrieveContract(runIndexIdentifier, sender, receivers));

        if (roomResponse.IsSuccessStatusCode) {
            var room = await roomResponse.Content.ReadFromJsonAsync<RoomRetrieveResponseContract>();
            if (room is { RoomId: not null }) {
                Console.WriteLine($"Room ID: {room.RoomId}");
                return room;
            }
        }
        Console.WriteLine("Failed to retrieve room information.");
        return new RoomRetrieveResponseContract(runIndexIdentifier, false, "Failed to retrieve room information", null, new BenchmarkTag());
    }
    public static async Task<HistoryResponseContract> FetchLastMessages(this HttpClient client,string runIndexIdentifier, string roomId) {
        var historyRetrieveContract = new HistoryRetrieveContract(runIndexIdentifier, roomId, lastMessageTimestamp, -1);
        lastMessageTimestamp = DateTime.Now;
        var historyResponse = await client.PostAsJsonAsync("/history", historyRetrieveContract);
        if (historyResponse.IsSuccessStatusCode) {
            var history = await historyResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();
            if (history != null && history.Messages.Count > 0) {
                foreach (var message in history.Messages) {
                    Console.WriteLine($"{message.Content}");
                }
                return history;
            }
        }
        return new HistoryResponseContract(runIndexIdentifier, [], false, new BenchmarkTag());
    }

    public static async Task<HistoryResponseContract> GetChatHistory(this HttpClient client, string runIndexIdentifier, string roomId) {
        // Fetch the last messages from the chat history
        lastMessageTimestamp = DateTime.Now.AddDays(-1);
        // Retrieve the chat history for the room
        var historyResponse = await client.PostAsJsonAsync("/history", new HistoryRetrieveContract(runIndexIdentifier, roomId, lastMessageTimestamp, 50));
        if (historyResponse.IsSuccessStatusCode) {
            // Read the response content as HistoryResponseContract
            var history = await historyResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();
            if (history != null && history.Messages.Count > 0) {
                Console.WriteLine("Chat History:");
                foreach (var message in history?.Messages) {
                    lastMessageTimestamp = message.Timestamp;
                    Console.WriteLine($"{message?.SendingUser?.Username}: {message?.Content}");
                }
                return history;
            }
            Console.WriteLine("No chat history found.");
        } else {
            Console.WriteLine("Failed to retrieve chat history.");
        }
        return new HistoryResponseContract(runIndexIdentifier, [], false, new BenchmarkTag());
    }
}