using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace j64.AlarmServer.Web.Repository
{
    public class SmartThingsConnection : HttpClient
    {
        public string BaseUrl = "https://graph.api.smartthings.com";
        public String LoginUrl { get { return BaseUrl + "/oauth/token/j_spring_security_check"; } }
        public String SmartAppInstallationsUri { get { return BaseUrl + "/ide/apps"; } }
        public String SmartAppResourceListUri { get { return BaseUrl + "/ide/app/getResourceList"; } }
        public String SmartAppCodeForResourceUri { get { return BaseUrl + "/ide/app/getCodeForResource"; } }
        public String CompileSmartAppUri { get { return BaseUrl + "/ide/app/compile"; } }
        public String SaveCompileSmartAppUri { get { return BaseUrl + "/ide/app/saveFromCode"; } }
        public String PublishSmartAppUri { get { return BaseUrl + "/ide/app/publishAjax"; } }
        public String UpdateSmartAppUri { get { return BaseUrl + "/ide/app/update"; } }
        public String EditSmartAppUri { get { return BaseUrl + "/ide/app/edit"; } }
        public String GetUuidUri { get { return BaseUrl + "/appIde/createUuid"; } }
        public String CreateDeviceTypeUri { get { return BaseUrl + "/ide/device/saveFromCode"; } }
        public String SaveCompileDeviceTypeUri { get { return BaseUrl + "/ide/device/compile"; } }
        public String PublishDeviceTypeUri { get { return BaseUrl + "/ide/device/publishAjax"; } }
        public String DeviceTypeList { get { return BaseUrl + "/ide/devices"; } }
        public String DeviceTypeResourceListUri { get { return BaseUrl + "/ide/device/getResourceList"; } }
        public String DeviceTypeCodeForResourceUri { get { return BaseUrl + "/ide/device/getCodeForResource"; } }

        public bool IsLoggedIn { get; set; } = false;

        public SmartThingsConnection() : base(new HttpClientHandler() { AllowAutoRedirect = false, CookieContainer = new CookieContainer() })
        {
        }

        public async Task<bool> Login(string user, string password)
        {
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("j_username", user));
            parms.Add(new KeyValuePair<string, string>("j_password", password));
            var content = new System.Net.Http.FormUrlEncodedContent(parms);

            var response = await PostAsync(LoginUrl, content);
            if (response.StatusCode != HttpStatusCode.Redirect || response.Headers.Location?.ToString().Contains("authfail") == true)
                return false;

            // Success
            IsLoggedIn = true;
            return true;
        }
    }
}