using System;
using System.Collections.Generic;
using LiteDB;

namespace Chat.Tests {
    public interface IBenchmarkDatabase {

        IEnumerable<Data> GetDataCollection();
        IEnumerable<BenchmarkReport> GetReportCollection();

        void InsertData(Data data);
        void InsertReport(BenchmarkReport report);
        List<Data> FindDataByRunIndex(string runIndexIdentifier);
        List<BenchmarkReport> GetAllReports();
        public int CountDataEntries();
        void Clear();
        int GetLastHttpStatusCode();
    }
}
