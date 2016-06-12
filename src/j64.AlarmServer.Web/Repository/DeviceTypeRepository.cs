using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace j64.AlarmServer.Web.Repository
{
    public class DeviceTypeRepository
    {
        private SmartThingsConnection myConnection = null;

        public string Id { get; set; }
        public string Code { get; set; }
        public string ResourceId { get; set; } = null;
        public string ResourceType { get; set; } = null;

        public DeviceTypeRepository(SmartThingsConnection smartThingsConnection)
        {
            myConnection = smartThingsConnection;
        }

        public DeviceTypeRepository(SmartThingsConnection smartThingsConnection, string appName, string fileName)
        {
            myConnection = smartThingsConnection;
            var t = CreateOrReplace(appName, fileName);
            t.Wait();
        }

        public async Task<bool> CreateOrReplace(string appName, string fileName)
        {
            string newCode = System.IO.File.ReadAllText(fileName);

            if (await Find(appName) == false)
            {
                if (await Create(newCode) == false)
                    throw new Exception("could not create device type");
            }
            else
            {
                if (await Update(newCode) == false)
                    throw new Exception("could not update device type");
            }

            if (await Publish() == false)
                throw new Exception("could not publish device type");

            return true;
        }

        public async Task<bool> Find(string deviceName)
        {
            // Find all the installed device types
            var response = await myConnection.GetAsync(myConnection.DeviceTypeList);
            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            var htmlResult = await response.Content.ReadAsStringAsync();
            foreach (Match m in Regex.Matches(htmlResult, "\\<a href=\"/ide/device/editor/([^\"]+)\".*?\\>\\s*(.+?)\\s*:\\s*(.+?)\\</a\\>", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (m.Groups[3].Value == deviceName)
                {
                    Id = m.Groups[1].Value;
                    break;
                }
            }
            if (Id == null)
                return false;

            // Get the resource list for the smart app
            response = await myConnection.GetAsync($"{myConnection.DeviceTypeResourceListUri.ToString()}?id={Id}");
            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            var jsonResult = response.Content.ReadAsStringAsync();
            var jobject = JObject.Parse("{ root: " + jsonResult.Result + " }");
            ResourceId = null;
            ResourceType = null;
            foreach (var jo in jobject.SelectToken("root").Children())
            {
                if (jo["type"].ToString() == "file")
                {
                    ResourceId = (string)jo["id"];
                    ResourceType = (string)jo["li_attr"]["resource-type"];
                    break;
                }
            }
            if (ResourceId == null)
                return false;

            // Get the code for this app
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("id", Id));
            parms.Add(new KeyValuePair<string, string>("resourceId", ResourceId));
            parms.Add(new KeyValuePair<string, string>("resourceType", ResourceType));
            var content = new System.Net.Http.FormUrlEncodedContent(parms);

            response = await myConnection.PostAsync(myConnection.DeviceTypeCodeForResourceUri, content);
            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            // Here is the code!
            Code = await response.Content.ReadAsStringAsync();

            // Success
            return true;
        }

        public async Task<bool> Create(string newCode)
        {
            // App id must have already been set
            if (Id != null || Code != null)
                throw new ArgumentException("The Id has already been set for this object");

            // Update the current code
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("content", newCode));
            parms.Add(new KeyValuePair<string, string>("fromCodeType", "code"));
            parms.Add(new KeyValuePair<string, string>("create", "Create"));
            var content = new System.Net.Http.FormUrlEncodedContent(parms);

            var response = await myConnection.PostAsync(myConnection.CreateDeviceTypeUri, content);
            if (response.StatusCode != HttpStatusCode.Found)
                return false;

            // Dig the Id out of the reponse location
            string[] segments = response.Headers.Location.Segments;
            Id = segments[segments.Length - 1];
            Code = newCode;

            // Success
            return true;
        }

        public async Task<bool> Update(string newCode)
        {
            // App id must have already been set
            if (Id == null || Code == null)
                throw new ArgumentException("The Id must be set before this device type can be updated");

            // UPdate the current code
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("id", Id));
            parms.Add(new KeyValuePair<string, string>("location", ""));
            parms.Add(new KeyValuePair<string, string>("code", newCode));
            parms.Add(new KeyValuePair<string, string>("resource", ResourceId));
            parms.Add(new KeyValuePair<string, string>("resourceType", ResourceType));
            var content = new System.Net.Http.FormUrlEncodedContent(parms);

            var response = await myConnection.PostAsync(myConnection.SaveCompileDeviceTypeUri, content);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"The app could not be updated. Status code {response.StatusCode}");

            var jsonResult = await response.Content.ReadAsStringAsync();
            var jobject = JObject.Parse("{ root: " + jsonResult + " }");
            if ((string)jobject["root"]["codeStatus"] != "NEW" && (string)jobject["root"]["codeStatus"] != "MODIFIED")
                throw new Exception("The status codeStatus returned from the update was not valid.");

            Code = newCode;
            return true;
        }

        public async Task<bool> Publish()
        {
            // App id must have already been set
            if (Id == null)
                throw new ArgumentException("The Id must be set before the device type can be published");

            // Update the current code
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("id", Id));
            parms.Add(new KeyValuePair<string, string>("scope", "me"));
            var content = new System.Net.Http.FormUrlEncodedContent(parms);

            var response = await myConnection.PostAsync(myConnection.PublishDeviceTypeUri, content);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"The device type could not be published. Status code {response.StatusCode}");

            return true;
        }
    }
}
