using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.IntegrationTests.Events;

public class SqlCounterLogger : ILogger
{
    private readonly string _categoryName;

    private static int _currentCounter;

    public SqlCounterLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public static int GetCounter()
    {
        return _currentCounter;
    }

    public static void ResetCounter()
    {
        _currentCounter = 0;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_categoryName == DbLoggerCategory.Database.Command.Name)
        {
            _currentCounter++;
        }
    }
}
