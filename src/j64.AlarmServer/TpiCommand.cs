using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer
{
    /// <summary>
    /// A command is issued by the application and sent to the TPI.  This 
    /// class will handle creating the command and putting it into the correct
    /// format for the TPI.
    /// </summary>
    public class TpiCommand
    {
        /// <summary>
        /// The data that is formatted properly for sending to the TPI
        /// </summary>
        public string TPIData { get; private set; }

        /// <summary>
        /// The command that is bieng sent
        /// </summary>
        public RequestCommand Command { get; private set; }

        /// <summary>
        /// The Data that is attached to the command in the format
        /// originally sent by the caller.
        /// </summary>
        public String CommandData { get; private set;  }

        /// <summary>
        /// Create a new command object
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public TpiCommand(RequestCommand command, String data = null)
        {
            Command = command;
            CommandData = data;

            if (data != null)
                TPIData = CheckSumHelper.AppendCheckSumAndEndOfRequest(command.GetStringValue() + data);
            else
                TPIData = CheckSumHelper.AppendCheckSumAndEndOfRequest(command.GetStringValue());
        }
    }

    public enum RequestCommand
    {
        [CommandValue("000")]
        Poll = 000,
        [CommandValue("001")]
        StatusReport = 001,
        [CommandValue("008")]
        DumpZoneTimers = 008,
        [CommandValue("005")]
        NetworkLogin = 005,
        [CommandValue("010")]
        SetTimeAndDate = 010,
        [CommandValue("020")]
        CommandOutputControl = 020,
        [CommandValue("030")]
        PartitionArmControl = 030,
        [CommandValue("031")]
        PartitionArmControlStayArm = 031,
        [CommandValue("032")]
        PartitionArmControlZeroEntryDelay = 032,
        [CommandValue("033")]
        PartitionArmControlWithCode = 033,
        [CommandValue("040")]
        PartitionDisarmControl = 040,
        [CommandValue("055")]
        TimeStampControl = 055,
        [CommandValue("056")]
        TimeBroadcastControl = 056,
        [CommandValue("057")]
        TemperatureBroadcastControl = 057,
        [CommandValue("060")]
        TriggerPanicAlarm = 060,
        [CommandValue("070")]
        SingleKeystrokePartition1 = 070,
        [CommandValue("071")]
        SendKeystrokeString = 071,
        [CommandValue("072")]
        EnterUserCodeProgramming = 072,
        [CommandValue("073")]
        EnterUserProgramming = 073,
        [CommandValue("074")]
        KeepAlive = 074,
        [CommandValue("200")]
        CodeSend = 200
    }
}
