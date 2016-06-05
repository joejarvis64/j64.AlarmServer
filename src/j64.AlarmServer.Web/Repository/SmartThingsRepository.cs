using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using j64.AlarmServer.Web.Models;

namespace j64.AlarmServer.Web.Repository
{
    public class SmartThingsRepository
    {

        /// <summary>
        /// This will propgate events occuring within the alarm system out to the smart things hub
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void AlarmSystem_ZoneChange(object sender, Zone z)
        {
            UpdateZone(new ZoneInfo(z));
        }

        /// <summary>
        /// This will propgate events occuring within the alarm system out to the smart things hub
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void AlarmSystem_PartitionChange(object sender, Partition p)
        {
            UpdatePartition(new PartitionInfo(p));
        }


        /// <summary>
        /// status information to the smart things zone
        /// </summary>
        /// <param name="zoneInfo"></param>
        public static void UpdateZone(ZoneInfo zoneInfo)
        {
            try
            {
                OauthInfo authInfo = OauthRepository.Get();

                string url = authInfo.endpoints[0].uri + $"/UpdateZone";

                var client = new System.Net.Http.HttpClient();

                System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                msg.Headers.Add("Authorization", $"Bearer {authInfo.accessToken}");

                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("Id", zoneInfo.Id.ToString()));
                parms.Add(new KeyValuePair<string, string>("Status", zoneInfo.Status));
                parms.Add(new KeyValuePair<string, string>("Name", zoneInfo.Name));
                msg.Content = new System.Net.Http.FormUrlEncodedContent(parms);
                var response = client.SendAsync(msg);
                response.Wait();

                if (response.Result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    MyLogger.LogError($"Error updating smart things zone {zoneInfo.Id} with status {zoneInfo.Status}");
                }
            }
            catch (Exception ex)
            {
                MyLogger.LogError($"Error updating smart things zone {zoneInfo.Id} with status {zoneInfo.Status}.  Exception was {MyLogger.ExMsg(ex)}");
            }
        }


        /// <summary>
        /// status information to the smart things zone
        /// </summary>
        /// <param name="zoneInfo"></param>
        public static void UpdatePartition(PartitionInfo partitionInfo)
        {
            try
            {
                OauthInfo authInfo = OauthRepository.Get();

                if (authInfo == null || authInfo.endpoints == null || authInfo.endpoints.Count == 0)
                {
                    MyLogger.LogError($"OAuth endpoints have not been created.  Cannot update smart things at this time");
                    return;
                }
                string url = authInfo.endpoints[0].uri + $"/UpdatePartition";

                var client = new System.Net.Http.HttpClient();

                System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                msg.Headers.Add("Authorization", $"Bearer {authInfo.accessToken}");

                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("Id", partitionInfo.Id.ToString()));
                parms.Add(new KeyValuePair<string, string>("Name", partitionInfo.Name));
                parms.Add(new KeyValuePair<string, string>("ReadyToArm", partitionInfo.ReadyToArm.ToString()));
                parms.Add(new KeyValuePair<string, string>("IsArmed", partitionInfo.IsArmed.ToString()));
                parms.Add(new KeyValuePair<string, string>("ArmingMode", partitionInfo.ArmingMode));
                parms.Add(new KeyValuePair<string, string>("AlarmOn", partitionInfo.AlarmOn.ToString()));
                msg.Content = new System.Net.Http.FormUrlEncodedContent(parms);
                var response = client.SendAsync(msg);
                response.Wait();

                if (response.Result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    MyLogger.LogError($"Error updating smart things partition {partitionInfo.Id} with status {partitionInfo.ArmingMode}");
                }
            }
            catch (Exception ex)
            {
                MyLogger.LogError($"Error updating smart things partition {partitionInfo.Id} with status {partitionInfo.ArmingMode}.  Exception was {MyLogger.ExMsg(ex)}");
            }
        }

        /// <summary>
        /// Install or Update Devices in the SmartThings App
        /// </summary>
        public static void InstallDevices(string hostString)
        {
            try
            {
                string[] h = hostString.Split(':');
                string j64Server = h[0];
                int j64Port = 80;
                if (h.Length > 1)
                    j64Port = Convert.ToInt32(h[1]);

                var hostName = System.Net.Dns.GetHostEntryAsync(System.Net.Dns.GetHostName());
                hostName.Wait();
                foreach (var i in hostName.Result.AddressList)
                {
                    if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        j64Server = i.ToString();
                        break;
                    }
                }

                OauthInfo authInfo = OauthRepository.Get();

                if (authInfo == null | authInfo.endpoints == null || authInfo.endpoints.Count == 0)
                {
                    MyLogger.LogError("OAuth endpoints have not been created. Cannot update smart things at this time");
                    return;
                }
                string url = authInfo.endpoints[0].uri + $"/installDevices";

                var client = new System.Net.Http.HttpClient();

                System.Net.Http.HttpRequestMessage msg = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                msg.Headers.Add("Authorization", $"Bearer {authInfo.accessToken}");

                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("j64Server", j64Server));
                parms.Add(new KeyValuePair<string, string>("j64Port", j64Port.ToString()));
                parms.Add(new KeyValuePair<string, string>("j64UserName", "admin"));
                parms.Add(new KeyValuePair<string, string>("j64Password", "Admin_01"));
                msg.Content = new System.Net.Http.FormUrlEncodedContent(parms);
                var response = client.SendAsync(msg);
                response.Wait();

                if (response.Result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    MyLogger.LogError($"Error installing smart things devices.  StatusCode was {response.Result.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MyLogger.LogError($"Error installing smart things devices.  Exception was {MyLogger.ExMsg(ex)}");
            }
        }
    }
}
