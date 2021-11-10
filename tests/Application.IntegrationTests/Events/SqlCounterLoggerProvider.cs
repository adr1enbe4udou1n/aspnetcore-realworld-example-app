using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.IntegrationTests.Events;

public class SqlCounterLoggerProvider : ILoggerProvider
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
