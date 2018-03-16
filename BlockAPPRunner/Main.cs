using Newtonsoft.Json;
using System;
using System.Threading;
using System.IO;
using BlockAPP_Core.Helpers;

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

                //var _JsonConfig = File.ReadAllText(AppContext.BaseDirectory + "\\config.json");
                //var _Config = JsonConvert.DeserializeObject<BlockAPP_Core.Models.Config>(_JsonConfig);

                //var _JsonGenesisBlock = File.ReadAllText(AppContext.BaseDirectory + "\\genesisBlock.json");
                //var _GenesisBlock = JsonConvert.DeserializeObject<BlockAPP_Core.Models.Block>(_JsonGenesisBlock);

                //var _GenerateId = _GenesisBlock.GetId();

                //BlockAPP_Core.Helpers.RSA.GenerateKeyPair();

                BlockAPP_Core.Db.DbContextManager.InitConnection();

                Console.ReadLine();
            }
        }
    }
}
