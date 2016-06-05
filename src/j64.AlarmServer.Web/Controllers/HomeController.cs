using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using j64.AlarmServer.Web.Models;

namespace j64.AlarmServer.Web.Controllers
{
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
