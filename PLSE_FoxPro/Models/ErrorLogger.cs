using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public interface IErrorLogger
    {
        void LogError(string message, [CallerMemberName]string source = null);
    }

    public class ConsoleErrorLogger : IErrorLogger
    {
        public void LogError(string message, [CallerMemberName] string source = null) => Console.WriteLine($"Error {message} occured in {source}");
    }
}
