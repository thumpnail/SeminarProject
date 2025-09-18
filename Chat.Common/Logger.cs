using System;
using System.IO;

namespace Chat.Common;

public class Logger {
    private readonly string _logFilePath;
    private readonly string logOrigin;
    private readonly bool _enabled;

    private readonly TextWriter _writer;

    public Logger(string origin) {
        logOrigin = origin;
        _enabled = Environment.GetEnvironmentVariable("DISABLE_FILE_LOG") != "1";
        if (_enabled) {
#if DEBUG
            _logFilePath = $"../../../../../log_{logOrigin}.log";
#elif  RELEASE
            _logFilePath = $"./log_{logOrigin}.log";
#endif

            _writer = new StreamWriter(_logFilePath);
        } else {
            _logFilePath = string.Empty;
            _writer = TextWriter.Synchronized(TextWriter.Null);
        }
    }

    public void Log(string message) {
        if (!_enabled) return;
        var logEntry = $"[{logOrigin}][{DateTime.Now:HH:mm:ss}] {message}";
        lock (_writer) {
            _writer.WriteLine(logEntry);
        }
    }
    public void Log(string from, string message) {
        if (!_enabled) return;
        var logEntry = $"[{logOrigin}][{DateTime.Now:HH:mm:ss}] [{from}] {message}";
        lock (_writer) {
            _writer.WriteLine(logEntry);
        }
    }
}