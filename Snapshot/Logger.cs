using System;
using System.IO;
using System.Text;

namespace Snapshot
{
    public class Logger
    {
        private readonly StringBuilder _sb = new StringBuilder();

        public void Log(string msg)
        {
            _sb.AppendLine(msg);
            Console.WriteLine(msg);
        }

        public void SaveLog(string path)
        {
            File.WriteAllText(path,_sb.ToString());
        }
    }
}