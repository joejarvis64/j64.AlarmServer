using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using j64.AlarmServer.WebApi.Models;
using Microsoft.AspNet.Authorization;

namespace j64.AlarmServer.WebApi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private AlarmSystem myAlarmSystem { get; set; }

        public HomeController(AlarmSystem alarmSystem)
        {
            myAlarmSystem = alarmSystem;
        }

        public IActionResult Index()
        {
            myAlarmSystem.DumpZoneTimers();
            return View(new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult Refresh()
        {
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        [Authorize(Roles = "ArmDisarm")]
        public IActionResult StayArm(int id)
        {
            myAlarmSystem.StayArmSystem(id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        [Authorize(Roles = "ArmDisarm")]
        public IActionResult AwayArm(int id)
        {
            myAlarmSystem.AwayArmSystem(id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        [Authorize(Roles = "ArmDisarm")]
        public IActionResult Disarm(int id)
        {
            myAlarmSystem.DisArmSystem(id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        [Authorize(Roles = "ArmDisarm")]
        public IActionResult SoundAlarm()
        {
            myAlarmSystem.SoundAlarm();
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        [Authorize(Roles = "ArmDisarm")]
        public IActionResult BypassZone(int id)
        {
            myAlarmSystem.BypassZone(id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }
    }
}
