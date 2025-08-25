using System;
using System.Collections.Generic;
using LiteDB;

namespace Chat.Tests {
    public class LiteBenchmarkDatabase : IBenchmarkDatabase {
        private readonly LiteDatabase db;
        private readonly ILiteCollection<Data> dataCollection;
        private readonly ILiteCollection<BenchmarkReport> reportCollection;

        private int lastHttpStatusCode;

        public LiteBenchmarkDatabase(string connectionString) {
            db = new LiteDatabase(connectionString);
            dataCollection = db.GetCollection<Data>("data");
            reportCollection = db.GetCollection<BenchmarkReport>("benchmarkReports");
        }

        public IEnumerable<Data> GetDataCollection() => dataCollection.FindAll();
        public IEnumerable<BenchmarkReport> GetReportCollection() => reportCollection.FindAll();

        public void InsertData(Data data) {
            lock (dataCollection) {
                dataCollection.Insert(data);
                lastHttpStatusCode = data.HttpStatusCode; // Track last HTTP status code
            }
        }

        public void InsertReport(BenchmarkReport report) {
            lock (reportCollection) {
                reportCollection.Insert(report);
            }
        }

        public List<Data> FindDataByRunIndex(string runIndexIdentifier) {
            lock (dataCollection) {
                return dataCollection.Find(x => x.RunIndexIdentifier == runIndexIdentifier).ToList();
            }
        }

        public List<BenchmarkReport> GetAllReports() {
            lock (reportCollection) {
                return reportCollection.FindAll().ToList();
            }
        }

        public int CountDataEntries() {
            lock (dataCollection) {
                return dataCollection.Count();
            }
        }

        public void Clear() {
            lock (dataCollection) {
                dataCollection.DeleteAll();
            }
            lock (reportCollection) {
                reportCollection.DeleteAll();
            }
        }

        public int GetLastHttpStatusCode() {
            return lastHttpStatusCode;
        }
    }
}
