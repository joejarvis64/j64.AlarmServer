using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer.Web.Models
{
    public class PartitionInfo
    {
        public PartitionInfo()
        {
        }

        public PartitionInfo(j64.AlarmServer.Partition p)
        {
            Id = p.Id;
            Name = p.Name;
            IsArmed = p.ArmedLed;
            SHMIntegrationEnabled = p.SHMIntegrationEnabled;
            ArmingMode = p.ArmingMode.ToString();
            AlarmOn = p.AlarmActive;
            ReadyToArm = p.ReadyLed;
            BypassActive = p.BypassLed;
            TroubleLedOn = p.TroubleLed;
            MemoryLedOn = p.MemoryLed;
            InProgramMode = p.ProgramLed;
            FireLedOn = p.FireLed;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public bool SHMIntegrationEnabled { get; set; }

        public bool IsArmed { get; set; }

        public string ArmingMode { get; set; }

        public bool AlarmOn { get; set; }

        public bool ReadyToArm { get; set; }

        public bool BypassActive { get; set; }

        public bool TroubleLedOn { get; set; }

        public bool MemoryLedOn { get; set; }

        public bool InProgramMode { get; set; }

        public bool FireLedOn { get; set; }
    }
}
