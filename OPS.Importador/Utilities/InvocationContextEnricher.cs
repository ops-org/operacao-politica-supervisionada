using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace OPS.Importador.Utilities
{
    public class InvocationContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.ContainsKey("SourceContext"))
            {
                var sourceContext = ((ScalarValue)logEvent.Properties["SourceContext"]).Value?.ToString();
                var callerFrame = GetCallerStackFrame(sourceContext);

                if (callerFrame != null)
                {
                    var methodName = callerFrame.GetMethod()?.Name;
                    var lineNumber = callerFrame.GetFileLineNumber();
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(callerFrame?.GetFileName());

                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CallerClassName", sourceContext));
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CallerMethodName", methodName));
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CallerFileName", fileName));
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CallerLineNumber", lineNumber));
                }
            }
        }

        private StackFrame GetCallerStackFrame(string className)
        {
            var trace = new StackTrace(true);
            var frames = trace.GetFrames();

            var callerFrame = frames.FirstOrDefault(f => f.GetMethod()?.DeclaringType?.FullName == className);

            return callerFrame;
        }
    }
}
