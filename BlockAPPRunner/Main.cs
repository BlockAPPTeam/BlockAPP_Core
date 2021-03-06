﻿using Newtonsoft.Json;
using System;
using System.Threading;
using System.IO;
using BlockAPP_Core.Helpers;
using System.Collections.Generic;
using BlockAPP_Core.Core.Network;
using BlockAPP_Core.Core.Network.Models;
using BlockAPP_Core.Core.Network.Enums;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

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

                Logger.InitLogger();

                BlockAPP_Core.Db.DbContextManager.InitConnection();
                var _TestKeys = RSA.GenerateKeyPair();

                BlockAPP_Core.SoftConfigs._LocalConfig = new BlockAPP_Core.Models.Config
                {
                    PrivateKey = _TestKeys.PriveteKey,
                    PublicKey = _TestKeys.PublicKey
                };

                Server _Server = new Server(10000);

                Server _Server2 = new Server(10001);

                _Server.Connect(IPAddress.Parse(Networking.GetLocalIP()), 10001);


                //StartServer();

                Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    _Server.SendData(@"{ id: ""TestIs"", Data: ""TestData"" }", _Server._WorkerSockets.First().Key);
                });
                Console.ReadLine();
            }
        }

       
    }
}
