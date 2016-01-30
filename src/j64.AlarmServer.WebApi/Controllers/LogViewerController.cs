using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.IO;
using j64.AlarmServer.WebApi.Model;

namespace j64.AlarmServer.WebApi.Controllers
{
    public class LogViewerController : Controller
    {
        public LogViewerController()
        {
        }

        public IActionResult Index()
        {
            // Save the most recent 500 lines from the file
            List<LogMessage> messages = new List<LogMessage>();

            string msg;
            using (var f = new StreamReader(new FileStream(MyLogger.LogFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                while ((msg = f.ReadLine()) != null)
                {
                    var split = msg.Split('@');
                    if (split.Length == 3)
                    {
                        messages.Insert(0, new LogMessage()
                        {
                            Type = split[0],
                            Date = Convert.ToDateTime(split[1]),
                            Message = split[2]
                        }
                        );
                    }

                    if (messages.Count > 500)
                        messages.RemoveAt(500);
                }
            }

            return View(messages);
        }

    }
}
