using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using j64.AlarmServer.WebApi.Models;
using Microsoft.AspNet.Authorization;

namespace j64.AlarmServer.WebApi.Controllers
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

        public IActionResult StayArm(int id)
        {
            ValidateSecurity.AllowLocalLan();
            myAlarmSystem.StayArmSystem(id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult AwayArm(int id)
        {
            ValidateSecurity.AllowLocalLan();
            myAlarmSystem.AwayArmSystem(id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult Disarm(int id)
        {
            ValidateSecurity.AllowLocalLan();
            myAlarmSystem.DisArmSystem(id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult SoundAlarm()
        {
            ValidateSecurity.AllowLocalLan();
            myAlarmSystem.SoundAlarm();
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult BypassZone(int id)
        {
            ValidateSecurity.AllowLocalLan();
            myAlarmSystem.BypassZone(id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }
    }
}
