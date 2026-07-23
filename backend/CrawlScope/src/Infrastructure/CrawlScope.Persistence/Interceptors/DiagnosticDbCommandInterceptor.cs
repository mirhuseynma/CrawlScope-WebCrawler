using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CrawlScope.Persistence.Interceptors
{
    public class DiagnosticDbCommandInterceptor(ILogger<DiagnosticDbCommandInterceptor> logger) : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, 
            CommandEventData eventData, 
            InterceptionResult<DbDataReader> result, 
            CancellationToken cancellationToken = default)
        {
            LogCommandStart(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            DbDataReader result, 
            CancellationToken cancellationToken = default)
        {
            LogCommandEnd(command, eventData);
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, 
            CommandEventData eventData, 
            InterceptionResult<object> result, 
            CancellationToken cancellationToken = default)
        {
            LogCommandStart(command);
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<object?> ScalarExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            object? result, 
            CancellationToken cancellationToken = default)
        {
            LogCommandEnd(command, eventData);
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, 
            CommandEventData eventData, 
            InterceptionResult<int> result, 
            CancellationToken cancellationToken = default)
        {
            LogCommandStart(command);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            int result, 
            CancellationToken cancellationToken = default)
        {
            LogCommandEnd(command, eventData);
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        private void LogCommandStart(DbCommand command)
        {
            logger.LogInformation("SQL Executing: {CommandText}", command.CommandText.Replace(Environment.NewLine, " "));
        }

        private void LogCommandEnd(DbCommand command, CommandExecutedEventData eventData)
        {
            if (eventData.Duration.TotalMilliseconds > 3000)
            {
                logger.LogWarning("SQL Executed (SLOW): {Duration}ms. Command: {CommandText}", 
                    eventData.Duration.TotalMilliseconds, 
                    command.CommandText.Replace(Environment.NewLine, " "));
            }
            else
            {
                logger.LogInformation("SQL Executed: {Duration}ms. Command: {CommandText}", 
                    eventData.Duration.TotalMilliseconds, 
                    command.CommandText.Replace(Environment.NewLine, " "));
            }
        }
    }
}
