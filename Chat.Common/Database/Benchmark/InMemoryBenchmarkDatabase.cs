namespace Chat.Tests {
    public class InMemoryBenchmarkDatabase : IBenchmarkDatabase {
        private readonly List<Data> dataCollection = new();
        private readonly List<BenchmarkReport> reportCollection = new();
        private int lastHttpStatusCode;

        public InMemoryBenchmarkDatabase(string connectionString) {
            // In-memory database, no initialization needed
        }

        public IEnumerable<Data> GetDataCollection() {
            return dataCollection;
        }
        public IEnumerable<BenchmarkReport> GetReportCollection() {
            return reportCollection;
        }
        public void InsertData(Data data) {
            lock (dataCollection) {
                dataCollection.Add(data);
                lastHttpStatusCode = data.HttpStatusCode; // Track last HTTP status code
            }
        }

        public void InsertReport(BenchmarkReport report) {
            lock (reportCollection) {
                reportCollection.Add(report);
            }
        }

        public List<Data> FindDataByRunIndex(string runIndexIdentifier) {
            lock (dataCollection) {
                return dataCollection.Where(x => x.RunIndexIdentifier == runIndexIdentifier).ToList();
            }
        }

        public List<BenchmarkReport> GetAllReports() {
            lock (reportCollection) {
                return new List<BenchmarkReport>(reportCollection);
            }
        }
        public int CountDataEntries() {
            lock (dataCollection) {
                return dataCollection.Count;
            }
        }

        public void Clear() {
            lock (dataCollection) {
                dataCollection.Clear();
            }
            lock (reportCollection) {
                reportCollection.Clear();
            }
        }

        public int GetLastHttpStatusCode() {
            return lastHttpStatusCode;
        }
    }
}
