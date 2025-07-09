using System;

namespace OPS.Core
{
    public static class ExceptionHelper
    {
        public static string ToFullDescriptionString(this Exception e)
        {
            if (e != null)
            {
                var sb = new System.Text.StringBuilder();

                sb.AppendLine("EXCEPTION INFORMATION");
                sb.AppendLine();
                sb.Append("Date: ");
                sb.AppendLine(DateTime.Now.ToString());
                sb.Append(new StackTraceHelper().ExpandStackTrace(e));

                return sb.ToString();
            }

            return string.Empty;
        }
    }

    internal class StackTraceHelper
    {
        internal string ExpandStackTrace(System.Exception exception)
        {
            int count = 0;
            return ExpandStackTrace(exception, ref count);
        }

        internal string ExpandStackTrace(System.Exception exception, ref int intExceptionCount)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);

            if (exception == null)
            {
                return string.Empty;
            }
            else
            {
                #region Process Exception

                var currentException = exception;	// Temp variable to hold InnerException object during the loop

                // Recursively call Expand Stack Trace to change the order or the exceptions.
                sb.AppendLine(ExpandStackTrace(currentException.InnerException, ref intExceptionCount));

                // Increment exception count.
                intExceptionCount++;

                // Write title information for the exception object.
                sb.AppendLine();
                sb.AppendFormat("{0} Exception Information", intExceptionCount);
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendFormat("Exception Type: {0}", currentException.GetType().FullName);
                sb.AppendLine();

                #region Loop through the public properties of the exception object and record their value

                // Loop through the public properties of the exception object and record their value.
                var aryPublicProperties = currentException.GetType().GetProperties();

                foreach (var p in aryPublicProperties)
                {
                    // Do not log information for the InnerException or StackTrace. This information is 
                    // captured later in the process.
                    if (p.Name != "InnerException" && p.Name != "StackTrace")
                    {
                        if (p.GetValue(currentException, null) == null)
                        {
                            sb.AppendFormat("{0}: NULL", p.Name);
                        }
                        else
                        {
                            sb.AppendFormat("{0}: {1}", p.Name, p.GetValue(currentException, null));
                        }

                        sb.AppendLine();
                    }
                }

                #endregion

                #region Record the Exception StackTrace

                // Record the StackTrace with separate label.
                if (currentException.StackTrace != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("StackTrace Information");
                    sb.AppendLine(currentException.StackTrace);
                }

                #endregion

                #endregion
            }

            return sb.ToString();
        }
    }
}
