using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace j64.AlarmServer
{
    /// <summary>
    /// An alarm system contains all of the zones and partitions managed by the Envisalink system.  The alarm system 
    /// will process events received from the TPI and handle them appropriately.  To use this class you should
    /// create a new instance and then start a session with the TPI:
    /// 
    ///     var a = new AlarmSystem();
    ///     a.StartSession(host, port, user, password);
    ///     
    /// This will get things rolling and allow you to see all of the commands and events that are being exchanged
    /// with the TPI.
    /// </summary>
    public class AlarmSystem
    {
        #region Properties
        /// <summary>
        /// The name of this system
        /// </summary>
        public string Name { get; set; } = "Alarm System";

        /// <summary>
        /// The host name of the envisalink server 
        /// </summary>
        public string Host
        {
            get
            {
                return _host;
            }
            set
            {
                _host = value;
                if (_tpiSocket != null)
                    _tpiSocket.Host = _host;
            }
        }
        private string _host = "envisalink";

        /// <summary>
        /// The port of the envisalink server
        /// </summary>
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                if (_tpiSocket != null)
                    _tpiSocket.Port = _port;
            }
        }
        private int _port = 4025;

        /// <summary>
        /// The user id to sign into envisalink with
        /// </summary>
        public string User { get; set; } = "user";

        /// <summary>
        /// The envisalink password
        /// </summary>
        public string Password { get; set; } = "user";

        /// <summary>
        /// The server where the j64 api alarm server is running
        /// (this needs to be refactored into another class!)
        /// </summary>
        public string j64Server { get; set; }

        /// <summary>
        /// The tcp port where the j64 api alarm server is running
        /// (this needs to be refactored into another class!)
        /// </summary>
        public string j64Port { get; set; }

        /// <summary>
        /// The code used to arm/disarm the system
        /// </summary>
        public string ArmingCode { get; set; }

        /// <summary>
        /// The partitions within the system
        /// </summary>
        public List<Partition> PartitionList { get; private set; } = new List<Partition>();

        /// <summary>
        /// The zones that are contained within this system
        /// </summary>
        public List<Zone> ZoneList { get; private set; } = new List<Zone>();

        /// <summary>
        /// Indicates whether the connection to TPI is currently active.
        /// </summary>
        [JsonIgnore]
        public bool IsConnectedToTpi
        {
            get
            {
                return _tpiSocket.IsConnected;
            }
        }
        #endregion

        #region Private properties
        private TPISocket _tpiSocket = null;
        #endregion

        #region Public Events
        /// <summary>
        /// Fired when a change in the zone status is detected
        /// </summary>
        public event EventHandler<Zone> ZoneChange;

        /// <summary>
        /// Fired when a change in the partition status is detected
        /// </summary>
        public event EventHandler<Partition> PartitionChange;
        #endregion

        #region Event Processor
        private void TpiSocket_ResponseReceived(object sender, TpiResponse response)
        {
            switch (response.ResponseType)
            {
                case TpiResponseType.System:
                    this.ProcessEvent(response);
                    break;

                case TpiResponseType.Partition:
                    // Find the partition and process it (ignore any partitions that are not defined in app settings)
                    Partition p = PartitionList.FirstOrDefault(x => x.Id == response.PartitionId);
                    if (p != null)
                        p.ProcessEvent(response);
                    break;

                case TpiResponseType.Zone:
                    // Find the zone and process it (ignore any zones that are not defined in app settings)
                    Zone z = ZoneList.FirstOrDefault(x => x.Id == response.ZoneId);
                    if (z != null)
                        z.ProcessEvent(response);
                    break;
            }
        }

        private void ProcessEvent(TpiResponse response)
        {
            // Send a password if requested
            if (response.Command == ResponseCommand.LoginInteraction && response.Data == "3")
            {
                _tpiSocket.ExecuteCommand(new TpiCommand(RequestCommand.NetworkLogin, Password));
                Thread.Sleep(500);
            }

            if (response.Command == ResponseCommand.LoginInteraction && response.Data == "1")
            {
                _tpiSocket.ExecuteCommand(new TpiCommand(RequestCommand.TimeStampControl, "1"));
                Thread.Sleep(1500);
                _tpiSocket.ExecuteCommand(new TpiCommand(RequestCommand.StatusReport));
            }

            if (response.Command == ResponseCommand.EnvisalinkZoneTimerDump)
            {
                for (int i = 0; i < 64; i++)
                {
                    Zone z = ZoneList.FirstOrDefault(x => x.Id == i+1);
                    if (z != null)
                        z.SecondsSinceLastClose = response.SecondsSinceOpened[i];
                }
            }
        }
        #endregion

        #region Start/Stop the Session
        public void StartSession(int pollInterval = 1200)
        {
            if (_tpiSocket != null)
                throw new Exception("TPI connection is already established.  You must Shutdown before starting a new session.");

            // Start listening to changes in the zones
            foreach (Zone z in ZoneList)
                z.ZoneChanged += Z_ZoneChanged;

            foreach (Partition p in PartitionList)
                p.PartitionChanged += P_PartitionChanged;

            // Establish the socket
            _tpiSocket = new TPISocket()
            {
                Host = this.Host,
                Port = this.Port,
                PollInterval = pollInterval
            };

            _tpiSocket.ResponseReceived += TpiSocket_ResponseReceived;
            _tpiSocket.StartSocket();
        }

        public void StopSession()
        {
            _tpiSocket.Disconnect();
        }
        #endregion

        #region Send Command Sequence
        public void StayArmSystem(int partitionId)
        {
            // Send the appropriate command sequence
            string cmd = String.Format("{0}", partitionId);
            TpiCommand tpiCmd = new TpiCommand(RequestCommand.PartitionArmControlStayArm, cmd);
            _tpiSocket.ExecuteCommand(tpiCmd);
        }

        public void AwayArmSystem(int partitionId)
        {
            // Send the appropriate command sequence
            string cmd = String.Format("{0}", partitionId);
            TpiCommand tpiCmd = new TpiCommand(RequestCommand.PartitionArmControl, cmd);
            _tpiSocket.ExecuteCommand(tpiCmd);
        }

        public void DisArmSystem(int partitionId)
        {
            if (String.IsNullOrEmpty(ArmingCode))
            {
                MyLogger.LogError("You must specify an arming code in the configuration");
                throw new Exception("You must specify an arming code in the configuration");
            }

            // Send the appropriate command sequence
            string cmd = String.Format("{0}{1}", partitionId, ArmingCode);
            TpiCommand tpiCmd = new TpiCommand(RequestCommand.PartitionDisarmControl, cmd);
            _tpiSocket.ExecuteCommand(tpiCmd);
        }

        public void SoundAlarm()
        {
            // Send the appropriate command sequence (this is the police alarm)
            TpiCommand tpiCmd = new TpiCommand(RequestCommand.TriggerPanicAlarm, "3");
            _tpiSocket.ExecuteCommand(tpiCmd);
        }
        
        public void BypassZone(int zoneId)
        {
            // Send the appropriate command sequence
            int partitionId = 1;
            string cmd = String.Format("{0}*1{1:D2}#", partitionId, zoneId);
            TpiCommand tpiCmd = new TpiCommand(RequestCommand.SendKeystrokeString, cmd);
            _tpiSocket.ExecuteCommand(tpiCmd);
        }

        public void DumpZoneTimers()
        {
            // Send the appropriate command sequence
            TpiCommand tpiCmd = new TpiCommand(RequestCommand.DumpZoneTimers);
            _tpiSocket.ExecuteCommand(tpiCmd);
        }
        #endregion

        #region Event Handlers
        private void Z_ZoneChanged(object sender, Zone e)
        {
            // Propogate to anybody listening to the whole alarm system
            if (ZoneChange != null)
                ZoneChange(this, e);
        }

        private void P_PartitionChanged(object sender, Partition e)
        {
            // Propogate to anybody listening to the whole alarm system
            if (PartitionChange != null)
                PartitionChange(this, e);
        }
    }
    #endregion
}
