using System;
using TaxiMobile.Lib.Infrastructure;

namespace TaxiMobile.Lib.Tests
{
    public class SimpleLogger : ILogger
    {
        
        public void LogError(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void StartStopwatch(string message)
        {
            Console.WriteLine(message);
        }

        public void StopStopwatch(string message)
        {
            Console.WriteLine(message);
        }
    }
}