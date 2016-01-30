using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer
{
    /// <summary>
    /// A partition within the alarm system is typically used to represent a collection of zones
    /// that can be armed/disarmed/monitored as a group.  Most residential homes do not create 
    /// more than one partition because they have a relatively small number of zones.  
    /// </summary>
    public class Partition
    {
        #region Properties
        /// <summary>
        /// The ID that that the TPI knows this system as
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The user assigned name for this partition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates the partition is ready to be armed
        /// </summary>
        [JsonIgnore]
        public bool ReadyLed { get; set; }

        /// <summary>
        /// Indicates that a zone has been bypassed
        /// </summary>
        [JsonIgnore]
        public bool BypassLed { get; set; }

        /// <summary>
        /// Indicates the partition is currently armed
        /// </summary>
        [JsonIgnore]
        public bool ArmedLed { get; set; }

        /// <summary>
        /// The mode the zone is armed in
        /// </summary>
        [JsonIgnore]
        public TpiArmedMode ArmingMode { get; set; } = TpiArmedMode.NotArmed;

        /// <summary>
        /// Indicates that an alarm is current sounding on the partition
        /// </summary>
        [JsonIgnore]
        public bool AlarmActive { get; set; }

        /// <summary>
        /// Indicates whether the chime is enabled
        /// </summary>
        [JsonIgnore]
        public bool ChimeEnabled { get; set; } = false;

        /// <summary>
        /// Memory
        /// </summary>
        [JsonIgnore]
        public bool MemoryLed { get; set; }

        /// <summary>
        /// Indicates some type of trouble within the partition
        /// </summary>
        [JsonIgnore]
        public bool TroubleLed { get; set; }

        /// <summary>
        /// The partition is in programming mode
        /// </summary>
        [JsonIgnore]
        public bool ProgramLed { get; set; }

        /// <summary>
        /// A fire has been detected in the partition
        /// </summary>
        [JsonIgnore]
        public bool FireLed { get; set; }

        /// <summary>
        /// The backlight is enabled
        /// </summary>
        [JsonIgnore]
        public bool BacklightLed { get; set; }

        /// <summary>
        /// The user id associated with the status (if any)
        /// /// </summary>
        [JsonIgnore]
        public int UserId { get; set; } = 0;
        #endregion

        #region Public Events
        /// <summary>
        /// Fired when a change in the partition is detected
        /// </summary>
        public event EventHandler<Partition> PartitionChanged;
        #endregion

        #region Process partition events
        public void ProcessEvent (TpiResponse response)
        {
            switch (response.Command)
            {
                case ResponseCommand.PartitionReady:
                    AlarmActive = false;
                    ReadyLed = true;
                    OnPartitionChanged();
                    break;

                case ResponseCommand.PartitionNotReady:
                    ReadyLed = false;
                    ArmingMode = TpiArmedMode.NotArmed;
                    OnPartitionChanged();
                    break;

                case ResponseCommand.KeypadLedStatePartition1Only:
                case ResponseCommand.KeypadLedflashStatePartition1Only:
                    BitArray ba = new BitArray(StringToByteArray(response.Data));
                    ReadyLed = ba[0];
                    ArmedLed = ba[1];
                    MemoryLed = ba[2];
                    BypassLed = ba[3];
                    TroubleLed = ba[4];
                    ProgramLed = ba[5];
                    FireLed = ba[6];
                    BacklightLed = ba[7];
                    OnPartitionChanged();
                    break;

                case ResponseCommand.PartitionInAlarm:
                    AlarmActive = true;
                    OnPartitionChanged();
                    break;

                case ResponseCommand.PartitionArmed:
                    ArmingMode = response.ArmingMode.Value;
                    ArmedLed = true;
                    OnPartitionChanged();
                    break;

                case ResponseCommand.PartitionDisarmed:
                    AlarmActive = false;
                    ArmedLed = false;
                    ArmingMode = TpiArmedMode.NotArmed;
                    OnPartitionChanged();
                    break;

                case ResponseCommand.ExitDelayinProgress:
                    ArmingMode = TpiArmedMode.ExitDelayInProgress;
                    OnPartitionChanged();
                    break;

                case ResponseCommand.EntryDelayinProgress:
                    ArmingMode = TpiArmedMode.EntryDelayInProgress;
                    OnPartitionChanged();
                    break;

                case ResponseCommand.ChimeEnabled:
                    ChimeEnabled = true;
                    break;

                case ResponseCommand.ChimeDisabled:
                    ChimeEnabled = false;
                    break;

                case ResponseCommand.TroubleLedon:
                    TroubleLed = true;
                    break;

                case ResponseCommand.TroubleLedoff:
                    TroubleLed = false;
                    break;

                case ResponseCommand.UserClosing:
                    UserId = response.UserId.Value;
                    break;

                case ResponseCommand.UserOpening:
                    UserId = response.UserId.Value;
                    break;

                // These are related to the partition but are not actively tracked in this API
                case ResponseCommand.PartitionReadyForceArmingEnabled:
                case ResponseCommand.KeypadLockout:
                case ResponseCommand.PgmOutputisinProgress:
                case ResponseCommand.InvalidAccessCode:
                case ResponseCommand.FunctionNotAvailable:
                case ResponseCommand.FailuretoArm:
                case ResponseCommand.PartitionisBusy:
                case ResponseCommand.SystemArminginProgress:
                case ResponseCommand.SpecialClosing:
                case ResponseCommand.PartialClosing:
                case ResponseCommand.SpecialOpening:
                    break;
            }
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private void OnPartitionChanged()
        {
            if (PartitionChanged != null)
                PartitionChanged(this, this);
        }
        #endregion
    }

    public enum TpiArmedMode
    {
        Away,
        Stay,
        ZeroEntryAway,
        ZeroEntryStay,
        NotArmed,
        ExitDelayInProgress,
        EntryDelayInProgress
    }
}
