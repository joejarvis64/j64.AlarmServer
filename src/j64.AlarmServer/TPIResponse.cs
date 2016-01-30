using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace j64.AlarmServer
{
    /// <summary>
    /// A response is issued by the TPI and sent back to the application via the socket.  This 
    /// class will handle parsing the command into a format that is more easily used by
    /// the application.
    public class TpiResponse : EventArgs
    {
        #region Private vars
        private const int CommandSize = 3;
        private const int ChecksumSize = 2;
        #endregion

        #region Properties
        /// <summary>
        /// The type of response received
        /// </summary>
        public TpiResponseType ResponseType
        {
            get
            {
                if (ZoneId != null)
                    return TpiResponseType.Zone;

                if (PartitionId != null)
                    return TpiResponseType.Partition;

                return TpiResponseType.System;
            }
        }

        /// <summary>
        /// The partition that this response is for
        /// </summary>
        public int? PartitionId { get; set; } = null;

        /// <summary>
        /// The zone this response is associated with
        /// </summary>
        public int? ZoneId { get; set; } = null;

        /// <summary>
        /// The user associated with the response
        /// </summary>
        public int? UserId { get; set; } = null;

        /// <summary>
        /// The Broadcast date within the response.  Only set with the time broadcase event
        /// </summary>
        public DateTime? DateBroadcast { get; set; } = null;

        /// <summary>
        /// The thermostat that is associated with the response
        /// </summary>
        public int? ThermostatId { get; set; } = null;

        /// <summary>
        /// The temperature that was read by the thermostat
        /// </summary>
        public int? Temperature { get; set; } = null;

        /// <summary>
        /// The command that was received
        /// </summary>
        public ResponseCommand Command { get; set; }

        /// <summary>
        /// The raw data associated with the response
        /// </summary>
        public String Data { get; set; }

        /// <summary>
        /// The date/time the response was sent
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The arming mode for the partition
        /// </summary>
        public TpiArmedMode? ArmingMode { get; set; } = null;

        /// <summary>
        /// The zone times across all zones
        /// </summary>
        public Int32[] SecondsSinceOpened { get; set; } = null;

        #endregion

        #region Constructor
        public TpiResponse(string data)
        {
            Parse(data);
            ParsePayload();
        }
        #endregion

        #region Parsing
        private void Parse(string data)
        {

            if (data == null)
            {
                data = "";
            }
            data = data.Replace("\0", "");
            //check if it's prefixed with date:
            if (data.Length > 9)
            {
                int number;
                Int32.TryParse(data.Substring(0, 1), out number);
                if (number > -1)
                {
                    //prefixed with date.
                    var date = data.Substring(0, 8);

                    DateTime tmp;
                    if (DateTime.TryParse(date, out tmp))
                    {
                        data = data.Substring(9); // remove date + space
                        Date = tmp;
                    }
                    else
                    {
                        Date = DateTime.Now;
                    }
                }
                else
                {
                    Date = DateTime.Now;
                }
            }
            else
                Date = DateTime.Now;

            //get ResponseEnum.
            ResponseCommand command;
            Enum.TryParse(data.Substring(0, CommandSize), out command);

            var numBytes = command.GetNumberOfBytes();

            var payload = ""; // can be 0-length

            if (numBytes > 0)
                payload = data.Substring(CommandSize, numBytes);

            var checksum = data.Substring(CommandSize + numBytes, ChecksumSize);

            if (!CheckSumHelper.VerifyChecksum(command, payload, checksum))
            {
                throw new Exception("Wrong checksum in response detected.");
            }

            Command = command;
            Data = payload;
        }

        private void ParsePayload()
        {
            // First, handle all of the events related to the alarm system
            switch (Command)
            {
                case ResponseCommand.TimeDateBroadcast:
                    DateBroadcast = DateTime.ParseExact(Data.Substring(0, 10), "hhmmMMDDYY", System.Globalization.CultureInfo.InvariantCulture);
                    break;

                case ResponseCommand.IndoorTemperatureBroadcast:
                case ResponseCommand.OutdoorTemperatureBroadcast:
                    ThermostatId = Convert.ToInt32(Data.Substring(0, 1));
                    Temperature = Convert.ToInt32(Data.Substring(1, 3));
                    break;

                case ResponseCommand.EnvisalinkZoneTimerDump:
                    SecondsSinceOpened = new Int32[64];
                    for (int i = 0; i < 64; i++)
                    {
                        string swappedBytes = Data.Substring((i * 4) + 2, 2) + Data.Substring(i * 4, 2);
                        int val = Convert.ToInt32(swappedBytes, 16);
                        if (val > 0)
                            SecondsSinceOpened[i] = (65535 - val) * 5;
                    }
                    break;

                case ResponseCommand.RingDetected:
                case ResponseCommand.DuressAlarm:
                case ResponseCommand.FKeyAlarm:
                case ResponseCommand.FKeyRestore:
                case ResponseCommand.AKeyAlarm:
                case ResponseCommand.AKeyRestoral:
                case ResponseCommand.PKeyAlarm:
                case ResponseCommand.PKeyRestore:
                case ResponseCommand.TwoWireSmokeAuxAlarm:
                case ResponseCommand.TwoWireSmokeAuxRestore:
                case ResponseCommand.PanelBatteryTrouble:
                case ResponseCommand.PanelBatteryTroubleRestore:
                case ResponseCommand.PanelACTrouble:
                case ResponseCommand.PanelACRestore:
                case ResponseCommand.SystemBellTrouble:
                case ResponseCommand.SystemBellTroubleRestoral:
                case ResponseCommand.FtcTrouble:
                case ResponseCommand.BufferNearFull:
                case ResponseCommand.GeneralSystemTamper:
                case ResponseCommand.GeneralSystemTamperRestore:
                case ResponseCommand.FireTroubleAlarm:
                case ResponseCommand.FireTroubleAlarmRestore:
                case ResponseCommand.VerboseTroubleStatus:
                case ResponseCommand.CodeRequired:
                case ResponseCommand.CommandOutputPressed:
                case ResponseCommand.MasterCodeRequired:
                case ResponseCommand.InstallersCodeRequired:
                    break;

                case ResponseCommand.KeypadLedStatePartition1Only:
                case ResponseCommand.KeypadLedflashStatePartition1Only:
                    PartitionId = 1;
                    break;

                case ResponseCommand.PartitionReady:
                case ResponseCommand.PartitionNotReady:
                case ResponseCommand.PartitionReadyForceArmingEnabled:
                case ResponseCommand.PartitionInAlarm:
                case ResponseCommand.PartitionDisarmed:
                case ResponseCommand.ExitDelayinProgress:
                case ResponseCommand.EntryDelayinProgress:
                case ResponseCommand.KeypadLockout:
                case ResponseCommand.PartitionFailedtoArm:
                case ResponseCommand.PgmOutputisinProgress:
                case ResponseCommand.ChimeEnabled:
                case ResponseCommand.ChimeDisabled:
                case ResponseCommand.InvalidAccessCode:
                case ResponseCommand.FunctionNotAvailable:
                case ResponseCommand.FailuretoArm:
                case ResponseCommand.PartitionisBusy:
                case ResponseCommand.SystemArminginProgress:
                case ResponseCommand.SpecialClosing:
                case ResponseCommand.PartialClosing:
                case ResponseCommand.SpecialOpening:
                case ResponseCommand.TroubleLedon:
                case ResponseCommand.TroubleLedoff:
                    PartitionId = Convert.ToInt32(Data.Substring(0, 1));
                    break;

                case ResponseCommand.PartitionArmed:
                    PartitionId = Convert.ToInt32(Data.Substring(0, 1));
                    ArmingMode = (TpiArmedMode)Convert.ToInt32(Data.Substring(1, 1));
                    break;

                case ResponseCommand.UserClosing:
                case ResponseCommand.UserOpening:
                    PartitionId = Convert.ToInt32(Data.Substring(0, 1));
                    UserId = Convert.ToInt32(Data.Substring(1, 4));
                    break;

                case ResponseCommand.ZoneAlarm:
                case ResponseCommand.ZoneAlarmRestore:
                case ResponseCommand.ZoneTamper:
                case ResponseCommand.ZoneTamperRestore:
                    PartitionId = Convert.ToInt32(Data.Substring(0, 1));
                    ZoneId = Convert.ToInt32(Data.Substring(1));
                    break;

                case ResponseCommand.ZoneFault:
                case ResponseCommand.ZoneFaultRestore:
                case ResponseCommand.ZoneOpen:
                case ResponseCommand.ZoneRestored:
                    ZoneId = Convert.ToInt32(Data);
                    break;

                case ResponseCommand.LoginInteraction:
                case ResponseCommand.CommandAcknowledge:
                    break;

                default:
                    MyLogger.LogError($"Invalid data returned [{Data}]");
                    break;
            }
        }
        #endregion
    }

    public enum TpiResponseType
    {
        System,
        Partition,
        Zone
    }

    public enum ResponseCommand
    {
        [CommandValue("500", 3)]
        CommandAcknowledge = 500,
        [CommandValue("501", 0)]
        CommandError = 501,
        [CommandValue("502", 3)]
        SystemError = 502,
        [CommandValue("505", 1)]
        LoginInteraction = 505,
        [CommandValue("510", 2)]
        KeypadLedStatePartition1Only = 510,
        [CommandValue("511", 2)]
        KeypadLedflashStatePartition1Only = 511,
        [CommandValue("550", 10)]
        TimeDateBroadcast = 550,
        [CommandValue("560", 2)]
        RingDetected = 560,
        [CommandValue("561", 4)]
        IndoorTemperatureBroadcast = 561,
        [CommandValue("562", 4)]
        OutdoorTemperatureBroadcast = 562,
        [CommandValue("601", 4)]
        ZoneAlarm = 601,
        [CommandValue("602", 4)]
        ZoneAlarmRestore = 602,
        [CommandValue("603", 4)]
        ZoneTamper = 603,
        [CommandValue("604", 4)]
        ZoneTamperRestore = 604,
        [CommandValue("605", 3)]
        ZoneFault = 605,
        [CommandValue("606", 3)]
        ZoneFaultRestore = 606,
        [CommandValue("609", 3)]
        ZoneOpen = 609,
        [CommandValue("610", 3)]
        ZoneRestored = 610,
        [CommandValue("615", 256)]
        EnvisalinkZoneTimerDump = 615,
        [CommandValue("620", 4)]
        DuressAlarm = 620,
        [CommandValue("621", 0)]
        FKeyAlarm = 621,
        [CommandValue("622", 0)]
        FKeyRestore = 622,
        [CommandValue("623", 0)]
        AKeyAlarm = 623,
        [CommandValue("624", 0)]
        AKeyRestoral = 624,
        [CommandValue("625", 1)]
        PKeyAlarm = 625,
        [CommandValue("626", 0)]
        PKeyRestore = 626,
        [CommandValue("631", 0)]
        TwoWireSmokeAuxAlarm = 631,
        [CommandValue("632", 0)]
        TwoWireSmokeAuxRestore = 632,
        [CommandValue("650", 1)]
        PartitionReady = 650,
        [CommandValue("651", 1)]
        PartitionNotReady = 651,
        [CommandValue("652", 2)]
        PartitionArmed = 652,
        [CommandValue("653", 1)]
        PartitionReadyForceArmingEnabled = 653,
        [CommandValue("654", 1)]
        PartitionInAlarm = 654,
        [CommandValue("655", 1)]
        PartitionDisarmed = 655,
        [CommandValue("656", 1)]
        ExitDelayinProgress = 656,
        [CommandValue("657", 1)]
        EntryDelayinProgress = 657,
        [CommandValue("658", 1)]
        KeypadLockout = 658,
        [CommandValue("659", 1)]
        PartitionFailedtoArm = 659,
        [CommandValue("660", 1)]
        PgmOutputisinProgress = 660,
        [CommandValue("663", 1)]
        ChimeEnabled = 663,
        [CommandValue("664", 1)]
        ChimeDisabled = 664,
        [CommandValue("670", 1)]
        InvalidAccessCode = 670,
        [CommandValue("671", 1)]
        FunctionNotAvailable = 671,
        [CommandValue("672", 1)]
        FailuretoArm = 672,
        [CommandValue("673", 1)]
        PartitionisBusy = 673,
        [CommandValue("674", 1)]
        SystemArminginProgress = 674,
        [CommandValue("700", 5)]
        UserClosing = 700,
        [CommandValue("701", 1)]
        SpecialClosing = 701,
        [CommandValue("702", 1)]
        PartialClosing = 702,
        [CommandValue("750", 5)]
        UserOpening = 750,
        [CommandValue("751", 1)]
        SpecialOpening = 751,
        [CommandValue("800", 0)]
        PanelBatteryTrouble = 800,
        [CommandValue("801", 0)]
        PanelBatteryTroubleRestore = 801,
        [CommandValue("802", 0)]
        PanelACTrouble = 802,
        [CommandValue("803", 0)]
        PanelACRestore = 803,
        [CommandValue("806", 0)]
        SystemBellTrouble = 806,
        [CommandValue("807", 0)]
        SystemBellTroubleRestoral = 807,
        [CommandValue("814", 0)]
        FtcTrouble = 814,
        [CommandValue("816", 0)]
        BufferNearFull = 816,
        [CommandValue("829", 0)]
        GeneralSystemTamper = 829,
        [CommandValue("830", 0)]
        GeneralSystemTamperRestore = 830,
        [CommandValue("840", 1)]
        TroubleLedon = 840,
        [CommandValue("841", 1)]
        TroubleLedoff = 841,
        [CommandValue("842", 0)]
        FireTroubleAlarm = 842,
        [CommandValue("843", 0)]
        FireTroubleAlarmRestore = 843,
        [CommandValue("849", 2)]
        VerboseTroubleStatus = 849,
        [CommandValue("900", 0)]
        CodeRequired = 900,
        [CommandValue("912", 1)]
        CommandOutputPressed = 912,
        [CommandValue("921", 0)]
        MasterCodeRequired = 921,
        [CommandValue("922", 0)]
        InstallersCodeRequired = 922
    }
}