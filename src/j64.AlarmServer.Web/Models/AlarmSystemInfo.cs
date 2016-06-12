using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace j64.AlarmServer.Web.Models
{
    public class AlarmSystemInfo
    {
        public AlarmSystemInfo()
        {
        }

        public AlarmSystemInfo(j64.AlarmServer.AlarmSystem myAlarmSystem)
        {
            Name = myAlarmSystem.Name;
            ArmingCode = myAlarmSystem.ArmingCode;

            Host = myAlarmSystem.Host;
            Port = myAlarmSystem.Port;
            User = myAlarmSystem.User;
            Password = myAlarmSystem.Password;
            IsConnected = myAlarmSystem.IsConnectedToTpi;
            j64Host = myAlarmSystem.j64Server;
            j64Port = myAlarmSystem.j64Port;

            myAlarmSystem.PartitionList.ForEach(p => Partitions.Add(new PartitionInfo(p)));
            myAlarmSystem.ZoneList.ForEach(z => Zones.Add(new ZoneInfo(z)));
        }

        public string Name { get; set; }
        public string ArmingCode { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string j64Host { get; set; }
        public string j64Port { get; set; }

        public bool IsConnected { get; set; } = false;

        public List<PartitionInfo> Partitions { get; set; } = new List<PartitionInfo>();
        public List<ZoneInfo> Zones { get; set; } = new List<ZoneInfo>();

        public List<LogMessage> LogMessages
        {
            get
            {
                // Save the most recent 500 lines from the file
                List<LogMessage> messages = new List<LogMessage>();

                string msg;
                using (var f = new StreamReader(new FileStream(MyLogger.LogFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    while ((msg = f.ReadLine()) != null)
                    {
                        var split = msg.Split('@');
                        if (split.Length == 3)
                        {
                            if (split[2].StartsWith("< 615:") == false &&
                                split[2].StartsWith("> 008:") == false &&
                                split[2].StartsWith("< 500:CommandAcknowledge   - 008") == false)
                            {
                                var dir = "na";
                                if (split[2].StartsWith(">"))
                                    dir = "in";
                                if (split[2].StartsWith("<"))
                                    dir = "out";

                                var command = split[2].Replace("<", "").Replace(">", "");

                                messages.Insert(0, new LogMessage()
                                {
                                    Type = split[0],
                                    Date = Convert.ToDateTime(split[1]),
                                    Direction = dir,
                                    Message = command
                                }
                                );
                            }
                        }

                        if (messages.Count > 500)
                            messages.RemoveAt(500);
                    }
                }

                return messages;
            }
        }
    }
}

