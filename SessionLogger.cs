using System.Text;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;

namespace MachineEnvironmentManager;

internal static class SessionLogger
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(SessionLogger));
    private static readonly MemoryAppender MemoryAppender = new();
    private static readonly object SyncRoot = new();
    private static bool initialized;

    public static event EventHandler<string>? MessageLogged;

    public static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        BasicConfigurator.Configure(MemoryAppender);
        initialized = true;
    }

    public static void Info(string message)
    {
        Initialize();
        Logger.Info(message);
        MessageLogged?.Invoke(null, message);
    }

    public static void Error(string message, Exception? exception = null)
    {
        Initialize();
        Logger.Error(message, exception);
        MessageLogged?.Invoke(null, exception == null ? message : $"{message}: {exception.Message}");
    }

    public static void SaveSession()
    {
        Initialize();

        LoggingEvent[] events = MemoryAppender.GetEvents();
        if (events.Length == 0)
        {
            return;
        }

        lock (SyncRoot)
        {
            string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);

            string filePath = Path.Combine(
                logDirectory,
                $"session-{DateTime.Now:yyyyMMdd-HHmmss-fff}.log");

            PatternLayout layout = new("%date %-5level %message%newline");
            layout.ActivateOptions();

            using StreamWriter writer = new(filePath, false, Encoding.UTF8);
            foreach (LoggingEvent logEvent in events)
            {
                writer.Write(layout.Format(logEvent));
            }
        }
    }
}
