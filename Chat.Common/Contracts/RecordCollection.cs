global using Identifier = string;
global using ID = string;

namespace Chat.Common.Contracts;

using Models;

public record BenchmarkTag {
    public string RunIndexIdentifier { get; set; }
    public List<BenchmarkSubTag> SubTags { get; set; } = new();
    public BenchmarkTag(){}
    public BenchmarkTag(string runIndexIdentifier, List<BenchmarkSubTag> SubTags) {
        this.SubTags = SubTags;
        this.RunIndexIdentifier = runIndexIdentifier;
    }
}
public record BenchmarkSubTag {
    public string Name { get; set; }
    public string Origin { get; set; }
    public long Timestamp { get; set; }
    public long ThreadMemory { get; set; }
    public long Memory { get; set; }
    public BenchmarkSubTag(){}
    public BenchmarkSubTag(string Origin, string Name, long Timestamp, long ThreadMemory, long Memory) {
        this.Name = Name;
        this.Origin = Origin;
        this.Timestamp = Timestamp;
        this.ThreadMemory = ThreadMemory;
        this.Memory = Memory;
    }
}

public record MessageSendContract(string runIndexIdentifier, string Sender, ID RoomId, string Content, DateTime Sent);
public record MessageSendResponseContract(string runIndexIdentifier, string Message, bool Success, BenchmarkTag Tag);

public record HistoryRetrieveContract(string runIndexIdentifier, ID RoomId, DateTime StartDate, int Limit = 50);
public record HistoryResponseContract(string runIndexIdentifier, List<Message> Messages, bool Success, BenchmarkTag Tag);

public record RoomRetrieveContract(string runIndexIdentifier, string Sender, string[] Receivers);
public record RoomRetrieveResponseContract(string runIndexIdentifier, bool Success, string Message, ID RoomId, BenchmarkTag Tag);