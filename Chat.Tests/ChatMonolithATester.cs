using System.Text;

using Chat.Common.Contracts;

using ChatApp.Client;
using LiteDB;
namespace Chat.Tests {
    public class ChatMonolithATester : BenchmarkTesterBase {
        public ChatMonolithATester(IBenchmarkDatabase benchmarkDatabase, int maxThreads, int maxMessages, int threadThrottle)
            : base(benchmarkDatabase, maxThreads, maxMessages, threadThrottle) {}

        protected override void ExecuteBenchmarkThread(IBenchmarkDatabase benchmarkDataCollection) {
            var client = new HttpClient {
                BaseAddress = new(Chat.Common.Addresses.CHAT_MONOLITH_SERVICE),
                Timeout = TimeSpan.FromSeconds(200)
            };
            var sender = usernames[rand.Next(usernames.Count)];
            var receiver = usernames[rand.Next(usernames.Count)];
            if (sender == receiver) return;

            // Get room information
            string roomId = GetRoomInformationAsync(benchmarkDataCollection, "monolith", "/room", client, sender, receiver, out DateTime getRoomStart, out float roomDuration, out BenchmarkTag roomTags);

            // Get chat history
            GetChatHistory(benchmarkDataCollection, "microservice", "/history", client, roomId, sender, receiver, out DateTime getHistoryStart, out float historyDuration, out BenchmarkTag historyTags);


            // Send messages
            for (int msgIdx = 0; msgIdx < msgCount; msgIdx++) {
                SendMessage(benchmarkDataCollection, "microservice", "/send", client, sender, roomId, msgIdx, receiver, out DateTime msgStart, out Task<MessageSendResponseContract> sendTask, out BenchmarkTag sendTags);
            }
        }
    }
}