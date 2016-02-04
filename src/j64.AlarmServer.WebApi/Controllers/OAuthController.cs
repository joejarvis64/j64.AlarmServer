using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using j64.AlarmServer.WebApi.Models;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNet.Authorization;

namespace j64.AlarmServer.WebApi.Controllers
{
    [Authorize(Roles = "ManageConfig")]
    public class OAuthController : Controller
    {
        public OAuthController()
        {
        }

        public IActionResult Index()
        {
            ValidateSecurity.AllowLocalLan();

            return View(OauthRepository.Get());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BeginAuth([Bind("clientKey", "secretKey")] OauthInfo authInfo)
        {
            string authorizedUrl = "http://" + this.Request.Host.Value + this.Url.Content("~/OAuth/Authorized");

            // Reset the token info
            authInfo.accessToken = null;
            authInfo.tokenType = null;
            authInfo.expiresInSeconds = 0;

            OauthRepository.Save(authInfo);

            string Url = $"https://graph.api.smartthings.com/oauth/authorize?response_type=code&scope=app&redirect_uri={authorizedUrl}&client_id={authInfo.clientKey}";

            return Redirect(Url);
        }

        public IActionResult Authorized(string code)
        {
            OauthInfo oai = OauthRepository.Get();

            if (code == null)
                return View(oai);

            oai.authCode = code;

            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

            var url = "https://graph.api.smartthings.com/oauth/token";

            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            parms.Add(new KeyValuePair<string, string>("code", oai.authCode));
            parms.Add(new KeyValuePair<string, string>("client_id", oai.clientKey));
            parms.Add(new KeyValuePair<string, string>("client_secret", oai.secretKey));
            string authorizedUrl = "http://" + this.Request.Host.Value + this.Url.Content("~/OAuth/Authorized");
            parms.Add(new KeyValuePair<string, string>("redirect_uri", authorizedUrl));

            var content = new System.Net.Http.FormUrlEncodedContent(parms);
            var response = client.PostAsync(url, content);
            response.Wait();

            if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ViewData.Add("GetTokenError", "Get Auth Code Error: " + response.Result.StatusCode.ToString());
                return View(oai);
            }

            // Save the interim result
            var val = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Result.Content.ReadAsStringAsync().Result);
            oai.accessToken = val["access_token"];
            oai.expiresInSeconds = Convert.ToInt32(val["expires_in"]);
            oai.tokenType = val["token_type"];
            OauthRepository.Save(oai);

            // Get the endpoint info
            client = new System.Net.Http.HttpClient();
            url = "https://graph.api.smartthings.com/api/smartapps/endpoints";

            System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            msg.Headers.Add("Authorization", $"Bearer {oai.accessToken}");

            response = client.SendAsync(msg);
            response.Wait();

            if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                ViewData.Add("GetTokenError", "Get EndPoints Error: " + response.Result.StatusCode.ToString());
                return View(oai);
            }

            string jsonString = response.Result.Content.ReadAsStringAsync().Result;
            oai.endpoints = JsonConvert.DeserializeObject<List<OauthEndpoint>>(jsonString);

            OauthRepository.Save(oai);

            // Install the Zones
            SmartThingsRepository.InstallDevices(this.Request.Host.Value);
            return View(oai);

        }
    }
}
