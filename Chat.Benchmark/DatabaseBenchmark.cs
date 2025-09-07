using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using Chat.Common;
namespace Chat.Benchmark;

[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[SimpleJob(RuntimeMoniker.NativeAot90)]
[RPlotExporter]
public class DatabaseBenchmark {
    IDatabase localDatabase = new LocalDatabase();
    IDatabase liteDatabase = new LiteBasedDatabase();
    string runIndexIdentifier = Guid.NewGuid().ToString();

    [Params(100,1000)]
    public int maxMessages = 1000;

    [GlobalSetup]
    public void Setup() {

    }

    [Benchmark]
    public void RunBenchmarkLocalDB() {
        var users = localDatabase.GetRoom(new(runIndexIdentifier,"benchmarkUser", new[] { "receiver1", "receiver2" }));
        var response = localDatabase.GetMessages(new(runIndexIdentifier,users.RoomId, DateTime.Now));
        for (int i = 0; i < maxMessages ; i++) {
            var message = localDatabase.InsertMessage(new(runIndexIdentifier,"benchmarkUser", users.RoomId, $"This is a benchmark message {i}", DateTime.Now));
        }
    }
    [Benchmark]
    public void RunBenchmarkLiteDB() {
        var users = liteDatabase.GetRoom(new(runIndexIdentifier,"benchmarkUser", new[] { "receiver1", "receiver2" }));
        var response = liteDatabase.GetMessages(new(runIndexIdentifier,users.RoomId, DateTime.Now));
        for (int i = 0; i < maxMessages ; i++) {
            var message = liteDatabase.InsertMessage(new(runIndexIdentifier,"benchmarkUser", users.RoomId, $"This is a benchmark message {i}", DateTime.Now));
        }
    }
}