namespace Chat.Tests;

public abstract class ATester {
    public abstract void Run();
    public abstract BenchmarkReport Report(out string reportResult);
}