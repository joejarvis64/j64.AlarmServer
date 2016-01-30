using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer
{
    /// <summary>
    /// A zone within the alarm system represents a set of devices that can be monitored.  For 
    /// example you may have a zone that represents all of your upstair windows or you may have
    /// a zone that is a single window upstairs.  The definition of the zone depends upon how
    /// the security system has been configured.
    /// </summary>
    public class Zone
    {
        #region Properties
        /// <summary>
        /// The ID that the TPI knows this zone as
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A user defined name for this zone
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of alarm zone (motion, contact, fire)
        /// </summary>
        public ZoneType ZoneType { get; set; }

        /// <summary>
        /// The current status of the zone
        /// </summary>
        [JsonIgnore]
        public ZoneStatus Status { get; private set; } = ZoneStatus.Closed;

        /// <summary>
        /// Indicates that the zone has an alarm sounding
        /// </summary>
        [JsonIgnore]
        public bool InAlarm { get; private set; } = false;

        /// <summary>
        /// The amount of time since the zone was closed
        /// </summary>
        [JsonIgnore]
        public int SecondsSinceLastClose { get; set; } = 0;
        #endregion

        #region Public Events
        /// <summary>
        /// Fired when a change in the zone is detected
        /// </summary>
        public event EventHandler<Zone> ZoneChanged;
        #endregion

        #region Process event
        /// <summary>
        /// Process events that are received against this zone
        /// </summary>
        /// <param name="response"></param>
        public void ProcessEvent(TpiResponse response)
        {
            switch (response.Command)
            {
                case ResponseCommand.ZoneAlarm:
                    Status = ZoneStatus.Alarm;
                    InAlarm = true;
                    OnZoneChanged();
                    break;

                case ResponseCommand.ZoneAlarmRestore:
                    Status = ZoneStatus.Closed;
                    InAlarm = false;
                    OnZoneChanged();
                    break;

                case ResponseCommand.ZoneTamper:
                    Status = ZoneStatus.Tamper;
                    OnZoneChanged();
                    break;

                case ResponseCommand.ZoneTamperRestore:
                    Status = ZoneStatus.Closed;
                    OnZoneChanged();
                    break;

                case ResponseCommand.ZoneFault:
                    Status = ZoneStatus.Fault;
                    OnZoneChanged();
                    break;

                case ResponseCommand.ZoneFaultRestore:
                    Status = ZoneStatus.Closed;
                    OnZoneChanged();
                    break;

                case ResponseCommand.ZoneOpen:
                    Status = ZoneStatus.Open;
                    OnZoneChanged();
                    break;

                case ResponseCommand.ZoneRestored:
                    Status = ZoneStatus.Closed;
                    OnZoneChanged();
                    break;
            }
        }

        private void OnZoneChanged()
        {
            if (ZoneChanged != null)
                ZoneChanged(this, this);
        }
        #endregion
    }

    /// <summary>
    /// The status that a zone can be in
    /// </summary>
    public enum ZoneStatus
    {
        Closed,
        Open,
        Fault,
        Alarm,
        Tamper
    }

    public enum ZoneType
    {
        Contact,
        Motion,
        Fire
    }
}