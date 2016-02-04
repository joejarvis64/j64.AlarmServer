using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.IO;
using j64.AlarmServer.WebApi.Models;
using Microsoft.AspNet.Authorization;

namespace j64.AlarmServer.WebApi.Controllers
{
    public class TpiLogViewComponent : ViewComponent
    {
        public TpiLogViewComponent()
        {
        }

    public IViewComponentResult Invoke()
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
                        if (split[2].StartsWith("< 615:") == false && 
                            split[2].StartsWith("> 008:") == false &&
                            split[2].StartsWith("< 500:CommandAcknowledge   - 008") == false)
                        {    
                            var dir = "na";
                            if (split[2].StartsWith(">"))
                                dir = "in";
                            if (split[2].StartsWith("<"))
                                dir = "out";
                            
                            var command = split[2].Replace("<","").Replace(">","");
                            
                            messages.Insert(0, new LogMessage()
                            {
                                Type = split[0],
                                Date = Convert.ToDateTime(split[1]),
                                Direction = dir,
                                Message = command
                            }
                            );
                        }
                    }

                    if (messages.Count > 500)
                        messages.RemoveAt(500);
                }
            }

            return View(messages);
        }

    }
}