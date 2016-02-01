using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer.WebApi.Models
{
    public class LogMessage
    {
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string Direction { get; set; }
        public string Message { get; set; }
    }
}
