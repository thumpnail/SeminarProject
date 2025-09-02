public class BenchmarkReport {
    public string RunIndexIdentifier { get; set; }
    public int ThreadCount { get; set; }
    public int MsgCount { get; set; }
    public int ThreadThrottle { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ServiceType { get; set; }
    public double Duration { get; set; }
    public int InvalidStatusCodeCount { get; set; } // Added property to track invalid HTTP status codes

    public List<BenchmarkSubReport> SubReports { get; set; } = new List<BenchmarkSubReport>();
    public List<Data> DataList { get; set; }
}
public class BenchmarkSubReport {
    public string Endpoint { get; set; }
    public int Count { get; set; }
    public float AvgDurationMs { get; set; }
    public float MinDurationMs { get; set; }
    public float MaxDurationMs { get; set; }
    public double AvgAllocatedBytes { get; set; }
    public long MaxAllocatedBytes { get; set; }
    public long MinAllocatedBytes { get; set; }
}