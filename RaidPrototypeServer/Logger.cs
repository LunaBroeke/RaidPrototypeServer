using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer
{
    public class Logger
    {
        public string name;
        public static MainWindow main;
        public void Log(string s)
        {
            LogMessage("Log", s);
        }

        public void LogWarning(string s)
        {
            LogMessage("Warning", s, "Yellow");
        }

        public void LogError(string s)
        {
            LogMessage("Error", s, "Red");
        }

        private void LogMessage(string logType, string message, string color = "Default")
        {
            if (main == null) throw new InvalidOperationException("Logger.main is not initialized");
            if (main.ConsoleBox == null) throw new InvalidOperationException("ConsoleBox is not initialized");

            if (main.ConsoleBox.InvokeRequired)
            {
                main.ConsoleBox.Invoke(new Action(() => LogMessage(logType, message, color)));
            }
            else
            {
                // Trim content if it gets too long
                const int maxLength = 10000;
                const int trimAmount = 500;
                if (main.ConsoleBox.Text.Length > maxLength)
                {
                    main.ConsoleBox.Text = main.ConsoleBox.Text.Substring(trimAmount);
                }

                // Set up the log message
                string logEntry = $"[{DateTime.Now.ToString("HH:mm:ss")}] [{logType}][{name}] {message}{Environment.NewLine}";

                // Append log to the console
                main.ConsoleBox.AppendText(logEntry);

                // Write to the log file
                File.AppendAllText($"log.txt", logEntry);
            }
        }
    }
}
