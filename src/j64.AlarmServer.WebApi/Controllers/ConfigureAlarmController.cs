using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using j64.AlarmServer.WebApi.Models;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Authorization;

namespace j64.AlarmServer.WebApi.Controllers
{
    [Authorize]
    public class ConfigureAlarmController : Controller
    {
        private AlarmSystem myAlarmSystem;

        public ConfigureAlarmController(AlarmSystem alarmSystem)
        {
            myAlarmSystem = alarmSystem;
        }

        public IActionResult Index()
        {
            return View(new AlarmSystemInfo(myAlarmSystem));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveChanges([Bind("Name", "ArmingCode", "Host", "Port", "User", "Password")] AlarmSystemInfo alarmInfo)
        {
            ValidateSecurity.AllowLocalLan();
            myAlarmSystem.Name = alarmInfo.Name;
            myAlarmSystem.ArmingCode = alarmInfo.ArmingCode;

            myAlarmSystem.Host = alarmInfo.Host;
            myAlarmSystem.Port = alarmInfo.Port;
            myAlarmSystem.User = alarmInfo.User;
            if (!String.IsNullOrEmpty(alarmInfo.Password) && alarmInfo.Password != "test123")
                myAlarmSystem.Password = alarmInfo.Password;

            for (int i = 0; i < Request.Form["partition.Id"].Count; i++)
            {
                var partition = myAlarmSystem.PartitionList.Find(p => p.Id.ToString() == Request.Form["partition.Id"][i]);

                partition.Name = Request.Form["partition.Name"][i];
            }

            for (int i = 0; i < Request.Form["zone.Id"].Count; i++)
            {
                var zone= myAlarmSystem.ZoneList.Find(p => p.Id.ToString() == Request.Form["zone.Id"][i]);

                zone.Name = Request.Form["zone.Name"][i];
                zone.ZoneType = (ZoneType)Enum.Parse(typeof(ZoneType), Request.Form["zone.ZoneType"][i]);
            }

            AlarmSystemRepository.Save(myAlarmSystem);

            // this will update the existing devices on the smartthings hub
            SmartThingsRepository.InstallDevices(this.Request.Host.Value);

            return View("index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult AddPartition()
        {
            ValidateSecurity.AllowLocalLan();

            var maxId = 1;
            if (myAlarmSystem.PartitionList.Count > 0)
                maxId = myAlarmSystem.PartitionList.Max(x => x.Id) + 1;

            myAlarmSystem.PartitionList.Add(
                new Partition()
                {
                    Id = maxId,
                    Name = $"Partition{maxId}"
                }
                );

            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult DeletePartition(int id)
        {
            ValidateSecurity.AllowLocalLan();

            var zonesDeleted = myAlarmSystem.PartitionList.RemoveAll(p => p.Id == id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult AddZone()
        {
            ValidateSecurity.AllowLocalLan();

            var maxId = 1;
            if (myAlarmSystem.ZoneList.Count > 0)
                maxId = myAlarmSystem.ZoneList.Max(x => x.Id) + 1;

            myAlarmSystem.ZoneList.Add(
                new Zone()
                {
                    Id = maxId,
                    Name = $"Zone{maxId}",
                    ZoneType = ZoneType.Contact
                }
                );

            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult DeleteZone(int id)
        {
            ValidateSecurity.AllowLocalLan();

            var zonesDeleted = myAlarmSystem.ZoneList.RemoveAll(z => z.Id == id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }
    }
}
