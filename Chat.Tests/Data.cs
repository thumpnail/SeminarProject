using Chat.Common.Contracts;

public class Data {
    public string RunIndexIdentifier { get; set; }
    public string Type { get; set; }
    public string Endpoint { get; set; }
    public DateTime Timestamp { get; set; }
    public float DurationMs { get; set; }
    public string Sender { get; set; }
    public string Receiver { get; set; }
    public BenchmarkTag Tag { get; set; }

    public Data(string runIndexIdentifier, string type, string endpoint, DateTime timestamp, float durationMs, string sender, string receiver, BenchmarkTag tag) {
        RunIndexIdentifier = runIndexIdentifier;
        Type = type;
        Endpoint = endpoint;
        Timestamp = timestamp;
        DurationMs = durationMs;
        Sender = sender;
        Receiver = receiver;
        Tag = tag;
    }
}