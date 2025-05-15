using System;
using System.Diagnostics;
using System.Threading;

namespace RevitTemplate.Utils
{
    /// <summary>
    /// Utility class for logging and error handling.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logs information about the current thread.
        /// </summary>
        /// <param name="name">The name of the process or operation.</param>
        public static void LogThreadInfo(string name = "")
        {
            Thread th = Thread.CurrentThread;
            Debug.WriteLine($"Task Thread ID: {th.ManagedThreadId}, Thread Name: {th.Name}, Process Name: {name}");
        }

        /// <summary>
        /// Handles and logs an exception.
        /// </summary>
        /// <param name="ex">The exception to handle.</param>
        public static void HandleError(Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            Debug.WriteLine($"Source: {ex.Source}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            
            // In a production environment, you might want to log to a file or a logging service
            // LogToFile(ex);
        }

        /// <summary>
        /// Logs a message with a timestamp.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogMessage(string message)
        {
            Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: {message}");
        }
    }
}
