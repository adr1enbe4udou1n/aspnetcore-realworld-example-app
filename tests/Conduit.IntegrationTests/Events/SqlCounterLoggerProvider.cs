using Microsoft.Extensions.Logging;

namespace Conduit.IntegrationTests.Events;

public sealed class SqlCounterLoggerProvider : ILoggerProvider
{
    public SqlCounterLoggerProvider()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SqlCounterLogger(categoryName);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}