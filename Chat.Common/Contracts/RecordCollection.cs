global using Identifier = string;
global using ID = string;

namespace Chat.Common.Contracts;

using Models;

public class BenchmarkTag {
    public List<BenchmarkSubTag> SubTags { get; set; } = new();
    public BenchmarkTag(){}
    public BenchmarkTag(List<BenchmarkSubTag> SubTags) {
        this.SubTags = SubTags;
    }
}
public class BenchmarkSubTag {
    public string Name { get; set; }
    public long Timestamp { get; set; }
    public long ThreadMemory { get; set; }
    public long Memory { get; set; }
    public BenchmarkSubTag(){}
    public BenchmarkSubTag(string Name, long Timestamp, long ThreadMemory, long Memory) {
        this.Name = Name;
        this.Timestamp = Timestamp;
        this.ThreadMemory = ThreadMemory;
        this.Memory = Memory;
    }
}

public record MessageSendContract(string Sender, ID RoomId, string Content, DateTime Sent);
public record MessageSendResponseContract(string Message, bool Success, BenchmarkTag Tag);

public record HistoryRetrieveContract(ID RoomId, DateTime StartDate, int Limit = 50);
public record HistoryResponseContract(List<Message> Messages, bool Success, BenchmarkTag Tag);

public record RoomRetrieveContract(string Sender, string[] Receivers);
public record RoomRetrieveResponseContract(bool Success, string Message, ID RoomId, BenchmarkTag Tag);