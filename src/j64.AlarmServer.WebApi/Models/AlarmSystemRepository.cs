using Newtonsoft.Json;
using System.IO;

namespace j64.AlarmServer.WebApi.Models
{
    public class AlarmSystemRepository
    {
        /// <summary>
        /// The full path to the json file that contains the configuration info
        /// </summary>
        public static string RepositoryFile { get; set; } = "AlarmSystemInfo.json";

        /// <summary>
        /// Get the current alarm system info
        /// </summary>
        /// <returns></returns>
        public static AlarmSystem Get()
        {
            AlarmSystem alarmSystem = null;

            if (File.Exists(RepositoryFile) == false)
            {
                // Create some default partitions and zones
                alarmSystem = new AlarmSystem();
                alarmSystem.PartitionList.Add(new Partition()
                {
                    Id = 1,
                    Name = "Home"
                });

                for (int i = 1; i <= 6; i++)
                {
                    alarmSystem.ZoneList.Add(new Zone()
                    {
                        Id = i,
                        Name = $"Zone {i}",
                        ZoneType = ZoneType.Contact
                    });
                }

                return alarmSystem;
            }

            // Read the settings now
            using (StreamReader file = System.IO.File.OpenText(RepositoryFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                alarmSystem = (AlarmSystem)serializer.Deserialize(file, typeof(AlarmSystem));
            }

            return alarmSystem;
        }

        /// <summary>
        /// Save a copy of the updates alarm info
        /// </summary>
        /// <param name="alarmSystem"></param>
        /// <returns></returns>
        public static void Save(AlarmSystem alarmSystem)
        {
            using (StreamWriter file = System.IO.File.CreateText(RepositoryFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, alarmSystem);
            }
        }
    }
}
