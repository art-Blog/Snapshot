using System;
using System.Diagnostics;

namespace Snapshot
{
    internal abstract class JobBase
    {
        private readonly Logger Logger = new Logger();
        private Stopwatch _stopWatch;
        private Stopwatch _stopWatch2;
        protected int SuccessCount;
        protected int FailureCount;
        protected string OutputFolder;
        protected string TargetSite;
        protected string[] Arguments;


        public void Run(string[] args)
        {
            try
            {
                WriteHistoryLog("=== Initial ===");
                Initial(args);

                WriteHistoryLog("=== Mission Start ===");
                Main();
                WriteHistoryLog("=== Mission End ===");

                SaveLog();
            }
            catch (Exception ex)
            {
                WriteHistoryLog("=== Mission Failed ===");
                WriteHistoryLog(ex.Message);
            }
        }

        protected abstract void Main();
        protected abstract void Initial(string[] args);
        protected abstract string GetTargetUrl(string argument);

        protected void WriteHistoryLog(string message)
        {
            Logger.Log(message);
        }

        private void SaveLog()
        {
            Logger.SaveLog($"{OutputFolder}\\Log.txt");
        }

        protected void StartTimeRecord()
        {
            _stopWatch = Stopwatch.StartNew();
        }

        protected void EndTimeRecord()
        {
            _stopWatch.Stop();
            var elapsed = _stopWatch.Elapsed;
            WriteHistoryLog($"成功: {SuccessCount}, 失敗: {FailureCount}。共花費時間：{Math.Floor(elapsed.TotalMinutes)}:{elapsed:ss\\.ff}");
        }

        protected void StartCaptureTiming()
        {
            _stopWatch2 = Stopwatch.StartNew();
        }

        protected void EndCaptureTiming(string message)
        {
            _stopWatch2.Stop();
            var elapsed = _stopWatch2.Elapsed;
            WriteHistoryLog($"{message} 花費時間：{Math.Floor(elapsed.TotalMinutes)}:{elapsed:ss\\.ff}");
        }
    }
}