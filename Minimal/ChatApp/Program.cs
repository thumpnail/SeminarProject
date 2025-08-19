// See https://aka.ms/new-console-template for more information

using Chat.Common;

using System.Net.Http.Json;

using Chat.Common.Contracts;

using ChatApp;
using ChatApp.UI;

using Terminal.Gui.App;

public class Program {
    public static HttpClient messagingClient = new() {
        BaseAddress = new Uri(Addresses.CHAT_MESSAGING_SERVICE)
    };
    public static HttpClient historyClient = new() {
        BaseAddress = new Uri(Addresses.CHAT_HISTORY_SERVICE)
    };
    public static void Main() {


        Console.WriteLine("Welcome to the ChatRoom Application!");
// Console.Write("Please provide your username(use ',' to separate):");
// var currentUser = Console.ReadLine();
// Console.Write("Receivers:");
// var currentReceiver = Console.ReadLine().Split(',').Select(r => r.Trim()).ToList();
//
        try {
        	var welcomeMsg = messagingClient.GetAsync("/").Result;
        	Console.WriteLine(welcomeMsg.Content.ReadAsStringAsync().Result);
        }
        catch (Exception e) {
        	Console.WriteLine("Error connecting to the chat service");
        }

        Application.Run<TermUI>().Dispose();
        Application.Shutdown();
/*
// Get chat history between current user and receiver
var historyRetrieveContract = new HistoryRetrieveContract(currentUser, currentReceiver, DateTime.Now.AddDays(-1), 50);
var historyResponse = await client.PostAsJsonAsync("/history", historyRetrieveContract);
if (historyResponse.IsSuccessStatusCode) {
    var history = await historyResponse.Content.ReadFromJsonAsync<HistoryResponseContract>();
    if (history != null && history.Messages.Count > 0) {
        Console.WriteLine("Chat History:");
        foreach (var message in history.Messages) {
            Console.WriteLine($"{message.Timestamp}: {message.SendingUser.Username}: {message.Content}");
        }
    } else {
        Console.WriteLine("No chat history found.");
    }
} else {
    Console.WriteLine("Failed to retrieve chat history.");
}
*/
//do {
//	input = Console.ReadLine();
//	// send message to api endpoint
//
//} while (input.Contains("!logout") == false);
    }
}