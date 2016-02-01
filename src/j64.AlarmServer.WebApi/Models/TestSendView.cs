using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer.WebApi.Models
{
    public class TestSendView
    {
        public int PartitionId { get; set; }
        public bool ReadyToArm { get; set; }
        public bool IsArmed { get; set; }
        public bool AlarmOn { get; set; }
        public string ArmingMode { get; set; }
        public int ZoneId { get; set; }
        public string ZoneStatus { get; set; }
        public string timeSinceLastClose { get; set; }
    }
}
