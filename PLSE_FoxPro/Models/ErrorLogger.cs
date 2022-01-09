using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public interface IErrorLogger
    {
        void LogError(Exception exception, [CallerMemberName]string source = null);
        void LogError(string message, [CallerMemberName]string source = null);
    }

    public class ConsoleErrorLogger : IErrorLogger
    {
        public void LogError(Exception exception, [CallerMemberName] string source = null) => Console.WriteLine($"Error {exception.Message} occured in {source}\n{exception.StackTrace}");

        public void LogError(string message, [CallerMemberName] string source = null) => Console.WriteLine($"Error {message} occured in {source}");
    }
}
