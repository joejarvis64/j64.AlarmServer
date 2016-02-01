using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace j64.AlarmServer.WebApi.Models
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

            myAlarmSystem.PartitionList.ForEach(p => Partitions.Add(new PartitionInfo(p)));
            myAlarmSystem.ZoneList.ForEach(z => Zones.Add(new ZoneInfo(z)));
        }

        public string Name { get; set; }
        public string ArmingCode { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public bool IsConnected { get; set; } = false;

        public List<PartitionInfo> Partitions { get; set; } = new List<PartitionInfo>();
        public List<ZoneInfo> Zones { get; set; } = new List<ZoneInfo>();
    }
}
