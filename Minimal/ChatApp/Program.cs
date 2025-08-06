// See https://aka.ms/new-console-template for more information

using Chat.Common;
using System.Net.Http.Json;
using Chat.Common.Contracts;
HttpClient client = new HttpClient {
	BaseAddress = new Uri(Addresses.CHAT_MESSAGING_SERVICE)
};

Console.WriteLine("Welcome to the ChatRoom Application!");

var input = "";
var welcomeMsg = await client.GetAsync("/");
Console.WriteLine(welcomeMsg.Content.ReadAsStringAsync().Result);
do {
	input = Console.ReadLine();
	// send message to api endpoint
	if (input.StartsWith("!login")) {
		Console.WriteLine("Logging in...");
		try {
			var parts = input.Split(' ');
			var username = parts[1];
			var password = parts[2];

			var loginResponse = await client.PostAsync("/login",
			JsonContent.Create(new LoginContract {
				Username = username,
				Password = password
			}));

			Console.WriteLine(loginResponse.Content.ReadFromJsonAsync<LoginResponseContract>().Result.ToJson());
		}
		catch (Exception e) {
			Console.WriteLine(e);
		}
	} else if (input.StartsWith("!logout")) {
		Console.WriteLine("Logging out...");
		return;
	} else {
		Console.WriteLine("Sending message...");
		// call api endpoint to send message
		Console.WriteLine($"Message sent: {input}");

		var messageResponse = await client.PostAsync("/send",
		JsonContent.Create(new MessageSendContract {
			Sender = "User",
			Receiver = "Receiver",
			Content = input
		}));

		Console.WriteLine(messageResponse.Content.ReadFromJsonAsync<MessageSendResponseContract>().Result.ToJson());
	}
} while (input.Contains("!logout") == false);
