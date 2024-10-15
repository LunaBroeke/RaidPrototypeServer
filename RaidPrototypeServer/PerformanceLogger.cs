using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Diagnostics;

namespace RaidPrototypeServer
{
    public static class PerformanceLogger
    {
        private const bool enabled = false;
        private static PerformanceCounter counter;
        private static string filepath = "cpu_usage_log.xlsx";

        public static void StartLoggingExcel()
        {
            if (enabled)
            {
                counter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName, true);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                InitializeExcelSheet();

                Thread loggingThread = new Thread(() =>
                {

                    while (true)
                    {

                        LogCpuUsageToExcel();
                        Thread.Sleep(1000);
                    }
                })
                { IsBackground = true };
                loggingThread.Start();
            }
        }

        private static void InitializeExcelSheet()
        {
            if (!File.Exists(filepath))
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Cpu Usage Log");
                    worksheet.Cells[1, 1].Value = "Time Stamp";
                    worksheet.Cells[1, 2].Value = "CPU Usage(%)";

                    worksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;
                    worksheet.Cells[1, 1, 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    File.WriteAllBytes(filepath, package.GetAsByteArray());
                }
            }
        }

        private static void LogCpuUsageToExcel()
        {
            try
            {
                float currentCpuUsage = counter.NextValue() / Environment.ProcessorCount;
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                using (var package = new ExcelPackage(new FileInfo(filepath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    int newRow = worksheet.Dimension.End.Row + 1;
                    worksheet.Cells[newRow, 1].Value = timestamp;
                    worksheet.Cells[newRow, 2].Value = currentCpuUsage;

                    package.Save();
                }
            }
            catch (InvalidOperationException e)

            {
                Server.logger.Log($"Please close {filepath}, we are unable to write while it is open");
            }
        }
    }
}
