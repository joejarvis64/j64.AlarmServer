using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace j64.AlarmServer.WebApi.Models
{
    public class ZoneInfo
    {
        public ZoneInfo()
        { 
        }

        public ZoneInfo(j64.AlarmServer.Zone z)
        {
            Id = z.Id;
            Name = z.Name;
            ZoneTypes t;
            Enum.TryParse(z.ZoneType.ToString(), out t);
            ZoneType = t;
            InAlarm = z.InAlarm;
            Status = z.Status.ToString();
            SecondsSinceLastClose = z.SecondsSinceLastClose;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public ZoneTypes ZoneType { get; set; }

        public string Status { get; set; }

        public bool InAlarm { get; set; }

        public int SecondsSinceLastClose { get; set; }

        public string TimeSinceLastClose
        {
            get
            {
                if (SecondsSinceLastClose == 0)
                {
                    if (Status == "Open")
                        return "Still open";
                    else
                        return "Closed Long Ago";
                }

                if (SecondsSinceLastClose < 60)
                    if (SecondsSinceLastClose == 1)
                        return String.Format("Closed {0} second ago", SecondsSinceLastClose);
                    else
                        return String.Format("Closed {0} seconds ago", SecondsSinceLastClose);

                if (SecondsSinceLastClose < 3600)
                {
                    int minutes = SecondsSinceLastClose / 60;
                    if (minutes == 1)
                        return String.Format("Closed {0} minute ago", minutes);
                    else
                        return String.Format("Closed {0} minutes ago", minutes);
                }

                int hours = SecondsSinceLastClose / 3600;
                if (hours == 1)
                    return String.Format("Closed {0} hour ago", hours);
                else
                    return String.Format("Closed {0} hours ago", hours);
            }
        }
    }

    public enum ZoneTypes
    {
        Contact,
        Motion,
        Smoke
    }
}
