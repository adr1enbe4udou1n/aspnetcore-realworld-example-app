using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Conduit.IntegrationTests.Events;

public class SqlCounterLogger : ILogger
{
    private readonly string _categoryName;

    private static int s_currentCounter;

    public SqlCounterLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public static int GetCounter()
    {
        return s_currentCounter;
    }

    public static void ResetCounter()
    {
        s_currentCounter = 0;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_categoryName == DbLoggerCategory.Database.Command.Name)
        {
            s_currentCounter++;
        }
    }
}