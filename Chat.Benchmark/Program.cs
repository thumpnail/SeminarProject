using BenchmarkDotNet.Running;

using Chat.Benchmark;
Console.WriteLine("Benchmark project");
var summary = BenchmarkRunner.Run<DatabaseBenchmark>();
Console.WriteLine(summary);