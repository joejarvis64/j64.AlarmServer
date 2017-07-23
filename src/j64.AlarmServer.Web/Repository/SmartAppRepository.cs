using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace j64.AlarmServer.Web.Repository
{
    public class SmartAppRepository
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string ResourceId { get; set; } = null;
        public string ResourceType { get; set; } = null;
        public string clientKey { get; set; } = null;
        public string secretKey { get; set; } = null;

        private SmartThingsConnection myConnection = null;

        public SmartAppRepository(SmartThingsConnection smartThingsConnection)
        {
            myConnection = smartThingsConnection;
        }

        public SmartAppRepository(SmartThingsConnection smartThingsConnection, string appName, string fileName)
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
                    throw new Exception("could not create smart app");
            }
            else
            {
                if (await Update(newCode) == false) 
                    throw new Exception("could not update smart app");
            }

            if (await EnableOauth() == false)
                throw new Exception("could not enable oauth in smart app");

            if (await Publish() == false)
                throw new Exception("could not publish smart app");

            return true;
        }

        public async Task<bool> Find(string smartAppName)
        {
            // Find all the installed smart apps
            var response = await myConnection.GetAsync(myConnection.SmartAppInstallationsUri);
            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            var htmlResult = await response.Content.ReadAsStringAsync();

            //foreach (Match m in Regex.Matches(htmlResult, "\\<a href=\"/ide/app/editor/([^\"]+)\".*?\\>\\<img .+?\\>\\s*(.+?)\\s*:\\s*(.+?)\\</a\\>", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            //{
            //    if (m.Groups[3].Value == smartAppName)
            //    {
            //        Id = m.Groups[1].Value;
            //        break;
            //    }
            //}

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlResult);

            var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//a");
            foreach (var node in htmlNodes)
            {
                if (node.Attributes["href"]?.Value.StartsWith("/ide/app/editor") == true)
                {
                    if (node.InnerText.Trim().Contains(smartAppName))
                    {
                        Id = node.Attributes["href"].Value.Replace("/ide/app/editor/", "");
                        break;
                    }
                }
            }

            if (Id == null)
                return false;

            // Get the resource list for the smart app
            response = await myConnection.GetAsync($"{myConnection.SmartAppResourceListUri.ToString()}?id={Id}");
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

            response = await myConnection.PostAsync(myConnection.SmartAppCodeForResourceUri, content);
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

            var response = await myConnection.PostAsync(myConnection.SaveCompileSmartAppUri, content);
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
                throw new ArgumentException("The Id must be set before this app can be updated");

            // UPdate the current code
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("id", Id));
            parms.Add(new KeyValuePair<string, string>("location", ""));
            parms.Add(new KeyValuePair<string, string>("code", newCode));
            parms.Add(new KeyValuePair<string, string>("resource", ResourceId));
            parms.Add(new KeyValuePair<string, string>("resourceType", ResourceType));
            var content = new System.Net.Http.FormUrlEncodedContent(parms);

            var response = await myConnection.PostAsync(myConnection.CompileSmartAppUri, content);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"The app could not be updated. Status code {response.StatusCode}");

            var jsonResult = await response.Content.ReadAsStringAsync();
            var jobject = JObject.Parse("{ root: " + jsonResult + " }");
            if ((string)jobject["root"]["codeStatus"] != "NEW")
                throw new Exception("The status codeStatus returned from the update was not valid.");

            Code = newCode;
            return true;
        }

        public async Task<bool> EnableOauth()
        {
            // App id must have already been set
            if (Id == null)
                throw new ArgumentException("The Id must be set before oauth can be enabled");

            // Find all the installed smart apps
            //clientKey = await myConnection.GetStringAsync(myConnection.GetUuidUri);
            //secretKey = await myConnection.GetStringAsync(myConnection.GetUuidUri);

            // Dig these out of the code code otherwise the oauth update will fail!
            string name = FindVarInSrc("name");
            string @namespace = FindVarInSrc("namespace");
            string author = FindVarInSrc("author");
            string description = FindVarInSrc("description");
            string iconUrl = FindVarInSrc("iconUrl");
            string iconX2Url = FindVarInSrc("iconX2Url");
            string iconX3Url = FindVarInSrc("iconX3Url");

            // Enable Oauth on the smart app
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("id", Id));
            parms.Add(new KeyValuePair<string, string>("name", name));
            parms.Add(new KeyValuePair<string, string>("namespace", @namespace));
            parms.Add(new KeyValuePair<string, string>("author", author));
            parms.Add(new KeyValuePair<string, string>("description", description));
            parms.Add(new KeyValuePair<string, string>("gitRepo.id", ""));
            //parms.Add(new KeyValuePair<string, string>("_isShared", ""));
            parms.Add(new KeyValuePair<string, string>("iconUrl", iconUrl));
            parms.Add(new KeyValuePair<string, string>("iconX2Url", iconX2Url));
            parms.Add(new KeyValuePair<string, string>("iconX3Url", iconX3Url));
            //parms.Add(new KeyValuePair<string, string>("clientId", clientKey));
            //parms.Add(new KeyValuePair<string, string>("clientSecret", secretKey));
            parms.Add(new KeyValuePair<string, string>("oauthEnabled", "true"));
            parms.Add(new KeyValuePair<string, string>("displayName", name));
            parms.Add(new KeyValuePair<string, string>("displayLink", ""));
            parms.Add(new KeyValuePair<string, string>("photoUrls", ""));
            parms.Add(new KeyValuePair<string, string>("videoUrls.0.videoUrl", ""));
            parms.Add(new KeyValuePair<string, string>("videoUrls.0.thumbnailUrl", ""));
            parms.Add(new KeyValuePair<string, string>("update", "Update"));
            var content = new System.Net.Http.FormUrlEncodedContent(parms);

            var response = await myConnection.PostAsync(myConnection.UpdateSmartAppUri, content);
            if (response.StatusCode != HttpStatusCode.Found)
                throw new Exception($"Oauth could not be enabled. Status code {response.StatusCode}");

            // Find all the installed smart apps
            response = await myConnection.GetAsync($"{myConnection.EditSmartAppUri}/{Id}");
            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            var htmlResult = await response.Content.ReadAsStringAsync();

            // Get the client key & secret key
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlResult);

            clientKey = htmlDoc.DocumentNode.SelectSingleNode("//input[@id='clientId']").Attributes["value"].Value;
            secretKey = htmlDoc.DocumentNode.SelectSingleNode("//input[@id='clientSecret']").Attributes["value"].Value;

            return true;
        }

        public async Task<bool> Publish()
        {
            // App id must have already been set
            if (Id == null)
                throw new ArgumentException("The Id must be set before the app can be published");

            // Update the current code
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("id", Id));
            parms.Add(new KeyValuePair<string, string>("scope", "me"));
            var content = new System.Net.Http.FormUrlEncodedContent(parms);

            var response = await myConnection.PostAsync(myConnection.PublishSmartAppUri, content);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception($"The app could not be published. Status code {response.StatusCode}");

            return true;
        }

        private string FindVarInSrc(string varname)
        {
            int bgn = Code.IndexOf($"{varname}:");
            if (bgn <= 0)
                return String.Empty;

            // Skip past the : "
            bgn += varname.Length + 3;

            int end = Code.IndexOf("\"", bgn);
            if (end <= 0)
                return String.Empty;

            return Code.Substring(bgn, end - bgn);
        }
    }
}
