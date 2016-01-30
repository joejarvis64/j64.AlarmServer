using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace j64.AlarmServer
{
    public class MyLogger
    {
        private static TextWriter _outFile = null;
        private static TextWriter OutFile
        {
            get
            {
                if (_outFile == null)
                {
                    if (File.Exists(LogFileName))
                        File.Delete(LogFileName);

                    _outFile = new StreamWriter(new FileStream(LogFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite));
                }

                return _outFile;
            }
        }

        public static string LogFileName {get; set; } = "LogMessages.txt";

        static MyLogger()
        {
        }
        
        public static void LogDebug(string msg)
        {
            OutFile.WriteLine($"DEBUG@{DateTime.Now}@{msg}");
            OutFile.Flush();
        }

        public static void LogInfo(string msg)
        {
            OutFile.WriteLine($"INFO@{DateTime.Now}@{msg}");
            OutFile.Flush();
        }

        public static void LogError(string msg)
        {
            OutFile.WriteLine($"ERROR@{DateTime.Now}@{msg}");
            OutFile.Flush();
        }

        public static string ExMsg(Exception ex)
        {
            string msg = "";
            while (ex != null)
            {
                msg += ex.Message + ":";
                ex = ex.InnerException;
            }

            return msg;
        }
    }
}
