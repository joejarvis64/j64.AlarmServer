using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using j64.AlarmServer.WebApi.Models;

namespace j64.AlarmServer.WebApi.Controllers
{
    public class TestSendController : Controller
    {
        public TestSendController()
        {
        }

        public IActionResult Index()
        {
            return View(new TestSendView());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PartitionTest([Bind("PartitionId", "ReadyToArm", "IsArmed", "ArmingMode", "AlarmOn")] TestSendView sendInfo)
        {
            ValidateSecurity.AllowLocalLan();

            // Find the partition info
            var pi = AlarmSystemRepository.Get().PartitionList.Find(x => x.Id == sendInfo.PartitionId);
            if (pi == null)
                throw new Exception("Invalid Partition ID");

            SmartThingsRepository.UpdatePartition(new PartitionInfo()
            {
                Id = sendInfo.PartitionId,
                Name = pi.Name,
                ReadyToArm = sendInfo.ReadyToArm,
                IsArmed = sendInfo.IsArmed,
                ArmingMode = sendInfo.ArmingMode,
                AlarmOn = sendInfo.AlarmOn
            });
            
            return View("index", sendInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ZoneTest([Bind("ZoneId", "ZoneStatus")] TestSendView sendInfo)
        {
            ValidateSecurity.AllowLocalLan();

            var zi = AlarmSystemRepository.Get().ZoneList.Find(x => x.Id == sendInfo.ZoneId);
            if (zi == null)
                throw new Exception("Invalid Zone ID");

            SmartThingsRepository.UpdateZone(new ZoneInfo()
            {
                Id = sendInfo.ZoneId,
                Name = zi.Name,
                Status = sendInfo.ZoneStatus
            });

            return View("index", sendInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RefreshDevices(TestSendView sendInfo)
        {
            ValidateSecurity.AllowLocalLan();

            SmartThingsRepository.InstallDevices(this.Request.Host.Value);
            return View("index", sendInfo);
        }
    }
}
