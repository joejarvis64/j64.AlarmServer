using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace j64.AlarmServer.Web.Models
{
    public class OauthInfo
    {
        // These are created when the smart app oauth is enabled
        public string clientKey { get; set; }
        public string secretKey { get; set; }

        // this is returned from the first step of auth
        public string authCode { get; set; }

        // this is returned from the second step and is used to call a service on the smart app
        public string accessToken { get; set; }
        public string tokenType { get; set; }
        public int expiresInSeconds { get; set; }


        // this is returned from the third step where we get the endpoint info
        public List<OauthEndpoint> endpoints { get; set; }
    }

    public class OauthEndpoint
    {
        public OauthClient oauthClient { get; set; }
        public string uri { get; set; }
        public string base_url { get; set; }
        public string url { get; set; }
    }

    public class OauthClient
    {
        public string clientId { get; set; }
        public string clientSecret { get; set; }
    }
}
