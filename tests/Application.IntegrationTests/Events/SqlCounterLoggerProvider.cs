using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.IntegrationTests.Events
{
    public class SqlCounterLoggerProvider : ILoggerProvider
    {
        private readonly Func<bool> _isEnabled;

        public SqlCounterLoggerProvider(Func<bool> isEnabled)
        {
            _isEnabled = isEnabled;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SqlCounterLogger(categoryName, _isEnabled);
        }

        public void Dispose()
        {
        }
    }
}