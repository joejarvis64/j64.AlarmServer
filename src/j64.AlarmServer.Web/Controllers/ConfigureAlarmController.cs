using j64.AlarmServer.Web.Models;
using j64.AlarmServer.Web.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace j64.AlarmServer.Web.Controllers
{
    [Authorize(Roles = "ManageConfig")]
    public class ConfigureAlarmController : Controller
    {
        private AlarmSystem myAlarmSystem;
        
        public ConfigureAlarmController(AlarmSystem alarmSystem)
        {
            myAlarmSystem = alarmSystem;
        }

        public IActionResult Index()
        {
            var asi = new AlarmSystemInfo(myAlarmSystem);
            //asi.Password = null;
            return View(asi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult SaveChanges([Bind("Name", "ArmingCode", "Host", "Port", "User", "Password")] AlarmSystemInfo alarmInfo)
        public ActionResult SaveChanges(AlarmSystemInfo alarmInfo)
        {
            if (ModelState.IsValid)
            {
                myAlarmSystem.Name = alarmInfo.Name;
                myAlarmSystem.ArmingCode = alarmInfo.ArmingCode;

                myAlarmSystem.Host = alarmInfo.Host;
                myAlarmSystem.Port = alarmInfo.Port;
                myAlarmSystem.User = alarmInfo.User;
                if (!String.IsNullOrEmpty(alarmInfo.Password))
                    myAlarmSystem.Password = alarmInfo.Password;

                myAlarmSystem.j64Server = alarmInfo.j64Host;
                myAlarmSystem.j64Port = alarmInfo.j64Port;

                for (int i = 0; i < Request.Form["partition.Id"].Count; i++)
                {
                    var partition = myAlarmSystem.PartitionList.Find(p => p.Id.ToString() == Request.Form["partition.Id"][i]);

                    partition.Name = Request.Form["partition.Name"][i];
                }

                for (int i = 0; i < Request.Form["zone.Id"].Count; i++)
                {
                    var zone = myAlarmSystem.ZoneList.Find(p => p.Id.ToString() == Request.Form["zone.Id"][i]);

                    zone.Name = Request.Form["zone.Name"][i];
                    zone.ZoneType = (ZoneType)Enum.Parse(typeof(ZoneType), Request.Form["zone.ZoneType"][i]);
                }

                AlarmSystemRepository.Save(myAlarmSystem);

                // this will update the existing devices on the smartthings hub
                SmartThingsRepository.InstallDevices(this.Request.Host.Value);

                // Add the partition/zone info to the alarm model we are returning
                alarmInfo = new AlarmSystemInfo(myAlarmSystem);
            }

            return View("index", alarmInfo);
        }

        public IActionResult AddPartition()
        {
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
            var zonesDeleted = myAlarmSystem.PartitionList.RemoveAll(p => p.Id == id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult AddZone()
        {
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
            var zonesDeleted = myAlarmSystem.ZoneList.RemoveAll(z => z.Id == id);
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }

        public IActionResult Findj64Address()
        {
            var asi = SmartThingsRepository.Determinej64ServerAddress(this.Request.Host.Value);
            myAlarmSystem.j64Server = asi.j64Server;
            myAlarmSystem.j64Port = asi.j64Port;
            return View("Index", new AlarmSystemInfo(myAlarmSystem));
        }
    }
}
