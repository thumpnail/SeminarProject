using System;
using System.Collections.Generic;

using Chat.Common.Contracts;

using LiteDB;

namespace Chat.Tests {
    public interface IBenchmarkDatabase {
        IEnumerable<Data> GetDataCollection();
        IEnumerable<BenchmarkReport> GetReportCollection();
        IEnumerable<BenchmarkTag> GetTagCollection();

        void InsertData(Data data);
        void InsertReport(BenchmarkReport report);
        void InsertTag(BenchmarkTag tag);
        List<Data> FindDataByRunIndex(string runIndexIdentifier);
        List<BenchmarkTag> FindTagsByRunIndex(string runIndexIdentifier);
        List<BenchmarkReport> GetAllReports();
        public int CountDataEntries();
        void Clear();
        int GetLastHttpStatusCode();
    }
}
