namespace Chat.Tests;

public class FlatMockBenchmarkDatabase : IBenchmarkDatabase {

    public FlatMockBenchmarkDatabase(string connectionString) {
    }
    public IEnumerable<Data> GetDataCollection() {
        Thread.Sleep(5);
        return Enumerable.Empty<Data>();
    }
    public IEnumerable<BenchmarkReport> GetReportCollection() {
        Thread.Sleep(5);
        return Enumerable.Empty<BenchmarkReport>();
    }
    public void InsertData(Data data) {
        Thread.Sleep(5);
    }
    public void InsertReport(BenchmarkReport report) {
        Thread.Sleep(5);
    }
    public List<Data> FindDataByRunIndex(string runIndexIdentifier) {
        Thread.Sleep(5);
        return [];
    }
    public List<BenchmarkReport> GetAllReports() {
        Thread.Sleep(5);
        return [];
    }
    public int CountDataEntries() {
        Thread.Sleep(5);
        return 0;
    }
    public void Clear() {
        Thread.Sleep(5);
    }
    public int GetLastHttpStatusCode() {
        Thread.Sleep(5);
        return 200;
    }
}