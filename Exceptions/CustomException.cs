using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace nopCommerceReplicatorServices.Exceptions
{
    public class CustomException : Exception
    {
        private static readonly ILogger<CustomException> _logger;

        static CustomException()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog();
            });
            _logger = loggerFactory.CreateLogger<CustomException>();
        }

        public CustomException(string message)
            : base(message)
        {
            LogException(message, null);
        }

        public CustomException(string message, Exception inner)
            : base(message, inner)
        {
            LogException(message, inner);
        }

        private void LogException(string message, Exception? innerException)
        {
            if (innerException != null)
            {
                _logger.LogError(innerException, message);
            }
            else
            {
                _logger.LogError(message);
            }
        }
    }
}
