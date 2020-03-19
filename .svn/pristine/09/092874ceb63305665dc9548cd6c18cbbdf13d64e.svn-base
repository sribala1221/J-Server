using System;
using Microsoft.Extensions.Logging;

//https://github.com/aspnet/Logging/issues/483#issuecomment-328355974
namespace ServerAPI.Extensions {
    public class TimedLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public TimedLogger(ILogger logger) => _logger = logger;

        public TimedLogger(ILoggerFactory loggerFactory) : this(new Logger<T>(loggerFactory)) { }

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter) =>
            _logger.Log(logLevel, eventId, state, exception,
                (s, ex) => $"[{DateTime.Now:HH:mm:ss}]: {formatter(s, ex)}");
    }
}
