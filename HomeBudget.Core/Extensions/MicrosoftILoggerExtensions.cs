using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;
using IMicrosoftILogger = Microsoft.Extensions.Logging.ILogger;

namespace HomeBudget.Core.Extensions
{
    public static class MicrosoftILoggerExtensions
    {
        public static IMicrosoftILogger LogWithExecutionMemberName(
            this IMicrosoftILogger logger,
            string message,
            LogLevel logLevel = LogLevel.Information,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                [nameof(memberName)] = memberName,
                [nameof(sourceLineNumber)] = sourceLineNumber,
            }))
            {
                logger.Log(logLevel, message);
            }

            return logger;
        }
    }
}
