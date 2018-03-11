using Newtonsoft.Json;
using System;
using System.Threading;
using System.IO;

namespace BlockAPPRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Mutex mutex = new Mutex(false, "Global\\3853E6EA-4AEA-41BC-813A-E2802D898F81"))
            {
                if (!mutex.WaitOne(0, false))
                {
                    return;
                }

                Console.WriteLine("BlockAPP_Core.Helpers.Networking.GetLocalIP => " + BlockAPP_Core.Helpers.Networking.GetLocalIP());

                var _JsonConfig = File.ReadAllText(AppContext.BaseDirectory + "\\config.json");
                var _Config = JsonConvert.DeserializeObject<BlockAPP_Core.Models.Config>(_JsonConfig);

                Console.ReadLine();
            }
        }
    }
}
