using System.Text;

using Chat.Common.Contracts;

using ChatApp.Client;

using LiteDB;
namespace Chat.Tests {
    public class ChatMicroserviceATester : BenchmarkTesterBase {
        public ChatMicroserviceATester(IBenchmarkDatabase benchmarkDatabase, int maxThreads, int maxMessages, int threadThrottle)
            : base(benchmarkDatabase, maxThreads, maxMessages, threadThrottle, "microservice") {
            this.ServiceType = "microservice";
        }

        protected override void ExecuteBenchmarkThread(IBenchmarkDatabase benchmarkDatabase) {
            var messagingClient = new HttpClient {
                BaseAddress = new(Chat.Common.Addresses.CHAT_MESSAGING_SERVICE),
                Timeout = TimeSpan.FromSeconds(200)
            };
            var historyClient = new HttpClient {
                BaseAddress = new(Chat.Common.Addresses.CHAT_HISTORY_SERVICE),
                Timeout = TimeSpan.FromSeconds(200)
            };
            var sender = "";
            var receiver = "";
            do {
                sender = usernames[rand.Next(usernames.Count)];
                receiver = usernames[rand.Next(usernames.Count)];
            } while (sender == receiver);

            // Get room information
            var room =
                GetRoomInformationAsync(benchmarkDatabase,
                    "microservice",
                    "/room",
                    messagingClient,
                    sender, receiver,
                    out DateTime getRoomStart,
                    out float roomDuration,
                    out BenchmarkTag roomTags);

            // Get chat history
            GetChatHistory(benchmarkDatabase,
                "microservice",
                "/history",
                historyClient, room,
                sender, receiver,
                out DateTime getHistoryStart,
                out float historyDuration,
                out BenchmarkTag historyTags);

            // Send messages
            for (int msgIdx = 0; msgIdx < msgCount; msgIdx++) {
                SendMessage(benchmarkDatabase,
                    "microservice",
                    "/send",
                    messagingClient,
                    sender,
                    room,
                    msgIdx,
                    receiver,
                    out DateTime msgStart,
                    out Task<MessageSendResponseContract> sendTask,
                    out BenchmarkTag sendTags);
            }
        }
    }
}