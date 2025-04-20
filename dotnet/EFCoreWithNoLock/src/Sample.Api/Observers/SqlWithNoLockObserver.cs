using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.RegularExpressions;

namespace Sample.Api.Observers
{
    public class SqlWithNoLockObserver : IObserver<KeyValuePair<string, object>>
    {
        private const string WithNoLockTagCommand = $"-- {QueryableExtensions.WithNoLockTag}";
        private const string WithNoLockReplacement = "$1 $2 $3 WITH (NOLOCK)";

        private static readonly Regex WithNoLockRegex = new(
            @"\b(FROM|JOIN)\s+(\[?[a-zA-Z0-9_.]+\]?)\s+(AS\s+\[?[a-zA-Z0-9_]+\])",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (value.Key != RelationalEventId.CommandExecuting.Name)
            {
                return;
            }
            
            var commandEventData = (CommandEventData)value.Value;

            if (commandEventData.ExecuteMethod == DbCommandMethod.ExecuteNonQuery || !commandEventData.Command.CommandText.StartsWith(WithNoLockTagCommand))
            {
                return;
            }

            commandEventData.Command.CommandText = WithNoLockRegex.Replace(commandEventData.Command.CommandText, WithNoLockReplacement);
        }
    }
}