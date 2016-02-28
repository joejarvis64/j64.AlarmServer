using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using j64.AlarmServer;
using j64.AlarmServer.WebApi.Models;
using Microsoft.AspNet.Authorization;

namespace j64.AlarmServer.WebApi.Controllers
{
    public class MyResponse<T>
    {
        public string FromHost { get; set; }
        public string Route { get; set; }
        public T Response { get; set; }
    }
    
    public class AlarmDTO
    {
        public bool IsConnected;
        
        public List<PartitionInfo> Partitions;
        public List<ZoneInfo> Zones;
    }

    [Authorize(Roles = "ArmDisarm")]
    [Route("api/[controller]")]
    public class AlarmSystemController : Controller
    {
        private AlarmSystem myAlarmSystem;

        public AlarmSystemController(AlarmSystem alarmSystem)
        {
            myAlarmSystem = alarmSystem;
        }

        // GET: api/AlarmSystem
        [HttpGet]
        public IActionResult GetSystemStatus()
        {
            var asi = new AlarmSystemInfo(myAlarmSystem);
            var adto = new AlarmDTO() {
               IsConnected = asi.IsConnected,
               Partitions = asi.Partitions,
               Zones = asi.Zones
            };
            
            return new ObjectResult(new MyResponse<AlarmDTO>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = adto
            });
        }

        // GET: api/AlarmSystem/Partition
        [HttpGet("Partitions")]
        public IActionResult GetPartitions()
        {
            return new ObjectResult(new MyResponse<List<PartitionInfo>>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = new AlarmSystemInfo(myAlarmSystem).Partitions
            });
        }

        // GET: api/AlarmSystem/Partition/1
        [HttpGet("Partitions/{partitionId}")]
        public IActionResult GetPartition(int partitionId)
        {
            PartitionInfo p = (new AlarmSystemInfo(myAlarmSystem)).Partitions.FirstOrDefault(x => x.Id == partitionId);
            if (p == null)
                return HttpNotFound();

            return new ObjectResult(new MyResponse<PartitionInfo>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = p
            });
        }

        // GET api/AlarmSystem/Zone
        [HttpGet("Zones")]
        public IActionResult GetZones()
        {
            return new ObjectResult(new MyResponse<List<ZoneInfo>>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = new AlarmSystemInfo(myAlarmSystem).Zones
            });
        }

        // GET api/AlarmSystem/Zone/5
        [HttpGet("Zones/{zoneId}")]
        public IActionResult GetZone(int zoneId)
        {
            ZoneInfo z = (new AlarmSystemInfo(myAlarmSystem)).Zones.FirstOrDefault(x => x.Id == zoneId);
            if (z == null)
                return HttpNotFound();

            return new ObjectResult(new MyResponse<ZoneInfo>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = z
            });
        }

        [HttpGet("StayArm/{partitionId}")]
        public IActionResult StayArm(int partitionId)
        {
            myAlarmSystem.StayArmSystem(partitionId);
            return new ObjectResult("issued");
        }

        [HttpGet("AwayArm/{partitionId}")]
        public IActionResult AwayArm(int partitionId)
        {
            myAlarmSystem.AwayArmSystem(partitionId);
            return new ObjectResult(new MyResponse<string>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = "Away Armed"
            });
        }

        [HttpGet("Disarm/{partitionId}")]
        public IActionResult Disarm(int partitionId)
        {
            myAlarmSystem.DisArmSystem(partitionId);

            return new ObjectResult(new MyResponse<string>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = "Disarmed"
            });
        }

        [HttpGet("SoundAlarm")]
        public IActionResult SoundAlarm()
        {
            myAlarmSystem.SoundAlarm();

            return new ObjectResult(new MyResponse<string>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = "Alarm Sounded"
            });
        }

        [HttpGet("BypassZone/{zoneId}")]
        public IActionResult BypassZone(int zoneId)
        {
            myAlarmSystem.BypassZone(zoneId);

            return new ObjectResult(new MyResponse<string>()
            {
                FromHost = Request.Host.Value,
                Route = Request.Path.Value,
                Response = "Bypassed"
            });
        }

    }
}
