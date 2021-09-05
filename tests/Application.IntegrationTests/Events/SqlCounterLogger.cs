using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.IntegrationTests.Events
{
    public class SqlCounterLogger : ILogger
    {
        private readonly string _categoryName;

        private readonly Func<bool> _isEnabled;

        public static int CurrentCounter { get; set; }

        public SqlCounterLogger(string categoryName, Func<bool> isEnabled)
        {
            _categoryName = categoryName;
            _isEnabled = isEnabled;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _isEnabled();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel) && _categoryName == DbLoggerCategory.Database.Command.Name)
            {
                CurrentCounter++;
            }
        }
    }
}
