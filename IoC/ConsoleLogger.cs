using System;
using System.Collections.Generic;
using System.Text;

namespace ContainerTest
{
    public class ConsoleLogger : ILogger
    {
        private Config config;

        public ConsoleLogger(Config config)
        {
            this.config = config;
        }

        public void Log(string message)
        {
            Console.WriteLine("[{0}] {1}", config.AppName, message);
        }
    }
}
