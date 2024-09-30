using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer
{
    public static class ResourceMonitor
    {
        public static MainWindow main;
        public static void WriteResourceMonitor()
        {
            while (true)
            {
                Process currentProcess = Process.GetCurrentProcess();
                string s = string.Empty;
                long workingSet = currentProcess.WorkingSet64;
                s += $"Working Set (Physical Memory): {workingSet / (1024 * 1024)} MB\r\n";
                long privateMemory = currentProcess.PrivateMemorySize64;
                s += $"Private Memory Size: {privateMemory / (1024 * 1024)} MB\r\n";
                TimeSpan totalProcessorTime = currentProcess.TotalProcessorTime;
                s += $"Total CPU Time: {totalProcessorTime.TotalMilliseconds} ms\r\n";
                int threadCount = currentProcess.Threads.Count;
                s += $"Thread Count:{threadCount}\r\n";
                int clientCount = Server.players.Count;
                s += $"Connected Clients: {clientCount}";
                ApplyInfo(s);
                Thread.Sleep(500);
            }
        }

        private static void ApplyInfo(string m)
        {
            if (main.ResourcesBox.InvokeRequired)
            {
                main.ResourcesBox.Invoke(new Action(() => ApplyInfo(m)));
            }
            else
            {
                main.ResourcesBox.Text = m;
            }
        }
    }
}
