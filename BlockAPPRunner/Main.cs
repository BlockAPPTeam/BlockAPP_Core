using Newtonsoft.Json;
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

                //BlockAPP_Core.Db.DbContextManager.InitConnection();






                StartServer();

                timerGarbagePatrol = new Timer(GardageTimerAction);
                timerGarbagePatrol.Change(0, 600000);
                
                Console.ReadLine();
            }
        }

        static int ServerPort = 100000;


       static string _PrivateKey;
       static Timer timerGarbagePatrol;

        static Thread DataProcessThread = null;
        static Thread FullPacketDataProcessThread = null;
        static Server _Server;
        static Dictionary<String, MotherRawPackets> _ClientsRawPackets = new Dictionary<string, MotherRawPackets>();
        static Queue<FullPacket> _FullPackets = new Queue<FullPacket>();

        static Boolean _Close = false;

        static AutoResetEvent autoEvent;//mutex
        static AutoResetEvent autoEvent2;//mutex
        static void StartServer()
        {
            try
            {
                autoEvent = new AutoResetEvent(false); //the RawPacket data mutex
                autoEvent2 = new AutoResetEvent(false);//the FullPacket data mutex
                DataProcessThread = new Thread(NormalizeThePackets);
                FullPacketDataProcessThread = new Thread(ProcessRecievedData);

                //Create HostServer
                _Server = new Server();

                _Server.Start(ServerPort);//MySettings.HostPort);
                _Server.OnReceiveData += new Server.ReceiveDataCallback(OnDataReceived);
                _Server.OnClientConnect += new Server.ClientConnectCallback(NewClientConnected);
                _Server.OnClientDisconnect += new Server.ClientDisconnectCallback(ClientDisconnect);

                DataProcessThread.Start();
                FullPacketDataProcessThread.Start();
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }
        static void NormalizeThePackets()
        {
            if (_Server == null) return;

            while (_Server.IsListening)
            {
                autoEvent.WaitOne(10000);

                lock (_ClientsRawPackets)
                {
                    foreach (var _MRawPacket in _ClientsRawPackets.Values)
                    {
                        if (_MRawPacket.GetItemCount == 0) continue;

                        try
                        {
                            Byte[] _PacketStorage = new byte[11264];//good for 10 full packets(10240) + 1 remainder(1024)

                            RawPackets _NewRawPacket;

                            while (true)
                            {
                                if (_MRawPacket.GetItemCount == 0)
                                    break;

                                int _HoldLength = 0;

                                if (_MRawPacket.BytesRemaining > 0)
                                {
                                    Copy(_MRawPacket.Remainder, 0, _PacketStorage, 0, _MRawPacket.BytesRemaining);
                                }
                                _HoldLength = _MRawPacket.BytesRemaining;

                                for (int i = 0; i < 10; i++)
                                {
                                    _NewRawPacket = _MRawPacket.GetTopItem;
                                    Copy(_NewRawPacket.DataChunk, 0, _PacketStorage, _HoldLength, _NewRawPacket.DataLength);
                                    _HoldLength += _NewRawPacket.DataLength;

                                    if (_MRawPacket.GetItemCount == 0)
                                    {
                                        break;
                                    }
                                }

                                int _ActualPackets = 0;

                                #region PACKET_SIZE 1024
                                if (_HoldLength >= 1024)//make sure we have at least one packet in there
                                {
                                    _ActualPackets = _HoldLength / 1024;
                                    _MRawPacket.BytesRemaining = _HoldLength - (_ActualPackets * 1024);

                                    for (int i = 0; i < _ActualPackets; i++)
                                    {
                                        Byte[] _Temp = new byte[1024];
                                        Copy(_PacketStorage, i * 1024, _Temp, 0, 1024);
                                        lock (_FullPackets)
                                        {
                                            _FullPackets.Enqueue(new FullPacket(_MRawPacket.ClientId, _Temp));
                                        }
                                    }
                                }
                                else
                                {
                                    _MRawPacket.BytesRemaining = _HoldLength;
                                }

                                Copy(_PacketStorage, _ActualPackets * 1024, _MRawPacket.Remainder, 0, _MRawPacket.BytesRemaining);
                                #endregion

                                if (_FullPackets.Count > 0)
                                {
                                    autoEvent2.Set();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _MRawPacket.ClearList();
                            //string msg = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
                            // ToDo
                        }
                    }
                }

                if (_Close) break;
            }
        }
        static void ProcessRecievedData()
        {
            if (_Server == null) return;

            while (_Server.IsListening)
            {
                autoEvent2.WaitOne();

                try
                {
                    while (_FullPackets.Count > 0)
                    {
                        FullPacket _NewFullPacket;
                        lock (_FullPackets)
                        {
                            _NewFullPacket = _FullPackets.Dequeue();
                        }

                        UInt16 type = (ushort)(_NewFullPacket.Data[1] << 8 | _NewFullPacket.Data[0]);
                        switch (type)//Interrigate the first 2 Bytes to see what the packet TYPE is
                        {
                            case (UInt16)PacketType.TYPE_MyCredentials:
                                {
                                    PostUserCredentials(_NewFullPacket.ClientId, _NewFullPacket.Data);
                                }
                                break;
                            case (UInt16)PacketType.TYPE_Close:
                                ClientDisconnect(_NewFullPacket.ClientId);
                                break;
                            case (UInt16)PacketType.TYPE_Message:
                                {
                                    AssembleMessage(_NewFullPacket.ClientId, _NewFullPacket.Data);
                                }
                                break;
                            case (UInt16)PacketType.TYPE_ClientDisconnecting:
                                {
                                    // ToDo
                                }
                                break;
                            case (UInt16)PacketType.TYPE_HostExiting:
                                {
                                    // ToDo
                                }
                                break;
                            case (UInt16)PacketType.TYPE_MessageReceived:
                                {
                                    // ToDo
                                }
                                break;
                            case (UInt16)PacketType.TYPE_Registered:
                                {
                                    // ToDo
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ToDo
                }

                if (_Close)
                    break;
            }

            if (!_Close)
            {
                //if we got here then something went wrong, we need to shut down the service
                // ToDo
            }
        }

        static void PostUserCredentials(String ClientId, Byte[] _Data)
        {
            try
            {
                PacketData IncomingData = new PacketData();
                IncomingData = (PacketData)PacketFunctions.ByteArrayToStructure(_Data, typeof(PacketData));

                lock (_Server._WorkerSockets)
                {
                    if (PacketFunctions.Verify( IncomingData))
                    {
                        _Server._WorkerSockets[ClientId].Timestamp = IncomingData.Timestamp;
                        _Server._WorkerSockets[ClientId].Accepted = true;
                        _Server._WorkerSockets[ClientId].Signature = IncomingData.Signature.NormalizeChars();
                        _Server._WorkerSockets[ClientId].PublicKey = IncomingData.PublicKey.NormalizeChars();
                        
                        SendRegisteredMessage(ClientId);
                    }
                    else
                    {
                        ClientDisconnect(ClientId);
                    }
                }
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }
        static void SendRegisteredMessage(String ClientId)
        {
            try
            {
                PacketData xdata = new PacketData();

                xdata.Packet_Type = (UInt16)PacketType.TYPE_Registered;
                xdata.Packet_Size = (UInt16)Marshal.SizeOf(typeof(PacketData));

                xdata.SignPacket(_PrivateKey);

                Byte[] _Data = PacketFunctions.StructureToByteArray(xdata);
                _Server.SendMessage(ClientId, _Data);
            }
            catch { }
        }

        static StringBuilder _SB;
        static void AssembleMessage(String _ClientID, Byte[] _Data)
        {
            try
            {
                PacketData IncomingData = new PacketData();
                IncomingData = (PacketData)PacketFunctions.ByteArrayToStructure(_Data, typeof(PacketData));

                if (IncomingData.Verify() == false) return;

                switch (IncomingData.Packet_SubType)
                {
                    case (UInt16)PacketTypeSubmessage.SUBMSG_MessageStart:
                        {
                            if (_Server._WorkerSockets.ContainsKey(_ClientID))
                            {
                                _SB = new StringBuilder(PacketFunctions.NormalizeChars(IncomingData.Data));
                            }
                        }
                        break;
                    case (UInt16)PacketTypeSubmessage.SUBMSG_MessageBody:
                        {
                            _SB.Append(PacketFunctions.NormalizeChars(IncomingData.Data));
                        }
                        break;
                    case (UInt16)PacketTypeSubmessage.SUBMSG_MessageEnd:
                        {
                            _SB.Append(PacketFunctions.NormalizeChars(IncomingData.Data));

                            /****************************************************************/
                            //Now tell the client teh message was received!
                            PacketData _Responce = new PacketData();
                            _Responce.Packet_Type = (UInt16)PacketType.TYPE_MessageReceived;

                            Byte[] _ResponceData = PacketFunctions.StructureToByteArray(_Responce);

                            _Server.SendMessage(_ClientID, _ResponceData);
                        }
                        break;
                }
            }
            catch
            {
                // ToDo
            }
        }
        


        public static void GardageTimerAction(Object stateInfo)
        {
            try
            {
                CheckConnectionTimersGarbagePatrol();
            }
            catch { }
        }
        
        static void ShotdownServer()
        {
            if (_Server != null)
            {
                PacketData xdata = new PacketData();

                xdata.Packet_Type = (UInt16)PacketType.TYPE_HostExiting;
                xdata.Packet_SubType = 0;
                xdata.Packet_Size = 16;

                Byte[] _Data = PacketFunctions.StructureToByteArray(xdata);

                foreach (var C in _Server._WorkerSockets)
                {
                    _Server.SendMessage(C.Key, _Data);
                }
                Thread.Sleep(250);
            }

            _Close = true;
            try
            {
                if (timerGarbagePatrol != null)
                {
                    timerGarbagePatrol.Dispose();
                    timerGarbagePatrol = null;
                }
            }
            catch { }

            _Server.Stop();
        }


        static void CheckConnectionTimersGarbagePatrol()
        {
            List<String> ClientIDsToClear = new List<String>();
            
            lock (_Server._WorkerSockets)
            {
                foreach (var _Sock in _Server._WorkerSockets.Values)
                {
                    TimeSpan diff = DateTime.Now - PacketFunctions.UnixTimeStampToDateTime(_Sock.Timestamp);

                    if (diff.TotalSeconds >= 60 || _Sock.UserSocket.Connected == false)//10 minutes
                    {
                        ClientIDsToClear.Add(_Sock.Id);
                    }
                }
            }
            
            if (ClientIDsToClear.Count > 0)
            {
                foreach (var _Guid in ClientIDsToClear)
                {
                    SendMessageOfClientDisconnect(_Guid);

                    CleanupDeadClient(_Guid);
                    Thread.Sleep(5);
                }
            }
        }

        static void CleanupDeadClient(String _Guid)
        {
            try
            {
                lock (_ClientsRawPackets)
                {
                    if (_ClientsRawPackets.ContainsKey(_Guid))
                    {
                        _ClientsRawPackets[_Guid].ClearList();
                        _ClientsRawPackets.Remove(_Guid);
                    }
                }
            }
            catch (Exception ex)
            {
                // ToDo
            }

            try
            {
                lock (_Server._WorkerSockets)
                {
                    if (_Server._WorkerSockets.ContainsKey(_Guid))
                    {
                        _Server._WorkerSockets[_Guid].UserSocket.Close();
                        _Server._WorkerSockets.Remove(_Guid);
                    }
                }
            }
            catch { }
        }



        static void OnDataReceived(String _ClientId, Byte[] _Data, int _Size)
        {
            if (_ClientsRawPackets.ContainsKey(_ClientId))
            {
                _ClientsRawPackets[_ClientId].AddToList(_Data, _Size);
                
                autoEvent.Set();
            }
        }

        static void NewClientConnected(String _CLientId)
        {
            try
            {
                if (_Server._WorkerSockets.ContainsKey(_CLientId))
                {
                    lock (_ClientsRawPackets)
                    {
                        if (!_ClientsRawPackets.ContainsKey(_CLientId))
                        {
                            _ClientsRawPackets.Add(_CLientId, new MotherRawPackets(_CLientId));
                        }
                    }

                    SetNewConnectionData_FromThread(_CLientId);
                }
                else
                {
                    // ToDo UNKNOWN CONNECTIONID
                }
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }

        static void SetNewConnectionData_FromThread(String _CLientId)
        {
            try
            {
                if (_Server._WorkerSockets[_CLientId].UserSocket.Connected)
                {
                    RequestNewConnectionCredentials(_CLientId);
                }
                else
                {
                   // ToDo $"ISSUE!!!(RequestNewConnectionCredentials) UserSocket.Connected is FALSE from: {_CLientId}"
                }
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }





        static void ClientDisconnect(String _CLientId)
        {
            if (_Close) return;

            /*******************************************************/
            lock (_ClientsRawPackets)//Make sure we don't do this twice
            {
                if (!_ClientsRawPackets.ContainsKey(_CLientId))
                {
                    lock (_Server._WorkerSockets)
                    {
                        if (!_Server._WorkerSockets.ContainsKey(_CLientId))
                        {
                            return;
                        }
                    }
                }
            }
            /*******************************************************/

            try
            {
                RemoveClient_FromThread(_CLientId);
            }
            catch (Exception ex)
            {
                // ToDo
            }

            CleanupDeadClient(_CLientId);


            Thread.Sleep(10);
        }

        static void RemoveClient_FromThread(String _CLientId)
        {
            try
            {
                SendMessageOfClientDisconnect(_CLientId);
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }


        static void RequestNewConnectionCredentials(String _ClientId)
        {
            try
            {
                PacketData xdata = new PacketData();

                xdata.Packet_Type = (UInt16)PacketType.TYPE_RequestCredentials;
                xdata.Packet_Size = (UInt16)Marshal.SizeOf(typeof(PacketData));

                if (!_Server._WorkerSockets.ContainsKey(_ClientId)) return;

                lock (_Server._WorkerSockets)
                {
                    String _ClientAddr = ((IPEndPoint)_Server._WorkerSockets[_ClientId].UserSocket.RemoteEndPoint).Address.ToString();
                    _ClientAddr.CopyTo(0, xdata.Data, 0, _ClientAddr.Length);

                    Byte[] _Data = PacketFunctions.StructureToByteArray(xdata);

                    if (_Server._WorkerSockets[_ClientId].UserSocket.Connected)
                    {
                        _Server.SendMessage(_ClientId, _Data);
                    }
                }
            }
            catch { }
        }

        static void SendMessageOfClientDisconnect(String _ClientId)
        {
            try
            {
                PacketData xdata = new PacketData();

                xdata.Packet_Type = (UInt16)PacketType.TYPE_ClientDisconnecting;
                xdata.Packet_Size = (UInt16)Marshal.SizeOf(typeof(PacketData));

                Byte[] _Data = PacketFunctions.StructureToByteArray(xdata);
                _Server.SendMessage(_ClientId, _Data);

                // ToDo зачем?
            }
            catch { }
        }

      
        
        static unsafe void Copy(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
        {
            try
            {
                if (src == null || srcIndex < 0 || dst == null || dstIndex < 0 || count < 0)
                {
                    Console.WriteLine("Serious Error in the Copy function 1");
                    // ToDO
                    throw new System.ArgumentException();
                }

                int srcLen = src.Length;
                int dstLen = dst.Length;
                if (srcLen - srcIndex < count || dstLen - dstIndex < count)
                {
                    Console.WriteLine("Serious Error in the Copy function 2");
                    // ToDO
                    throw new System.ArgumentException();
                }
                
                fixed (byte* pSrc = src, pDst = dst)
                {
                    byte* ps = pSrc + srcIndex;
                    byte* pd = pDst + dstIndex;
                    
                    for (int i = 0; i < count / 4; i++)
                    {
                        *((int*)pd) = *((int*)ps);
                        pd += 4;
                        ps += 4;
                    }
                    
                    for (int i = 0; i < count % 4; i++)
                    {
                        *pd = *ps;
                        pd++;
                        ps++;
                    }
                }
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }
    }
}
