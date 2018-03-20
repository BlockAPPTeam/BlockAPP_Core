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

        string _PrivateKey;
       static Timer timerGarbagePatrol;

        static Thread DataProcessThread = null;
        static Thread FullPacketDataProcessThread = null;
        static Server _Server;
        static Dictionary<String, MotherRawPackets> _ClientsRawPackets = new Dictionary<string, MotherRawPackets>();
        static Queue<FullPacket> _FullPackets = new Queue<FullPacket>();

        static Boolean _Close = false;

        private static void StartServer()
        {
            try
            {
                autoEvent = new AutoResetEvent(false); //the RawPacket data mutex
                autoEvent2 = new AutoResetEvent(false);//the FullPacket data mutex
                DataProcessThread = new Thread(NormalizeThePackets);
                FullPacketDataProcessThread = new Thread(ProcessRecievedData);

                //Create HostServer
                _Server = new Server();

                svr.Listen(MyPort);//MySettings.HostPort);
                svr.OnReceiveData += new Server.ReceiveDataCallback(OnDataReceived);
                svr.OnClientConnect += new Server.ClientConnectCallback(NewClientConnected);
                svr.OnClientDisconnect += new Server.ClientDisconnectCallback(ClientDisconnect);

                DataProcessThread.Start();
                FullPacketDataProcessThread.Start();

                OnCommunications($"TCPiP Server is listening on port {MyPort}", INK.CLR_GREEN);
            }
            catch (Exception ex)
            {
                var exceptionMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                //Debug.WriteLine($"EXCEPTION IN: StartPacketCommunicationsServiceThread - {exceptionMessage}");
                OnCommunications($"EXCEPTION: TCPiP FAILED TO START, exception: {exceptionMessage}", INK.CLR_RED);
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

        StringBuilder _SB;
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
        
        private void ShotdownServer()
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


        private void CheckConnectionTimersGarbagePatrol()
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
        
        private void CleanupDeadClient(String _Guid)
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


        
        private void OnDataReceived(String _ClientId, Byte[] _Data, int _Size)
        {
            if (_ClientsRawPackets.ContainsKey(_ClientId))
            {
                _ClientsRawPackets[_ClientId].AddToList(_Data, _Size);
                
                autoEvent.Set();
            }
        }
        
        private void NewClientConnected(String _CLientId)
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
        
        private void SetNewConnectionData_FromThread(String _CLientId)
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


      


        private void ClientDisconnect(String _CLientId)
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

        private void RemoveClient_FromThread(String _CLientId)
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

        
        private void RequestNewConnectionCredentials(String _ClientId)
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

        private void SendMessageOfClientDisconnect(String _ClientId)
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

      

        #region WEB CONTROL
        private bool BrowserVersion()
        {
            Debug.WriteLine("CommunicationsDisplay.Version: " + CommunicationsDisplay.Version.ToString());

            if (CommunicationsDisplay.Version.Major < 9)
            {
                MessageBox.Show(this, "You must update your web browser to Internet Explorer 9 or greater to see the service output information!", "Message", MessageBoxButtons.OK);
                return false;
            }

            return true;
        }

        private delegate void OnCommunicationsDelegate(string str, INK iNK);
        private void OnCommunications(string str, INK iNK)
        {
            if (ValidBrowser == false)
            {
                System.Diagnostics.Debug.WriteLine("INVALID BROWSER, must update Internet Explorer to version 8 or better!!");
                return;
            }
            Int32 line = 0;
            //System.Diagnostics.Debug.WriteLine("~~~~~~ OnCommunications 1:");
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke(new OnCommunicationsDelegate(OnCommunications), new object[] { str, iNK });
                    return;
                }

                //  System.Diagnostics.Debug.WriteLine("~~~~~~ OnCommunications 2");
                HtmlDocument doc = CommunicationsDisplay.Document;
                line = 1;
                //System.Diagnostics.Debug.WriteLine("~~~~~~ OnCommunications 3");
                string style = String.Empty;
                if (iNK.Equals(INK.CLR_GREEN))
                    style = Properties.Settings.Default.StyleGreen;
                else if (iNK.Equals(INK.CLR_BLUE))
                    style = Properties.Settings.Default.StyleBlue;
                else if (iNK.Equals(INK.CLR_RED))
                    style = Properties.Settings.Default.StyleRed;
                else if (iNK.Equals(INK.CLR_PURPLE))
                    style = Properties.Settings.Default.StylePurple;
                else
                    style = Properties.Settings.Default.StyleBlack;
                line = 2;
                //System.Diagnostics.Debug.WriteLine("~~~~~~ OnCommunications 4");
                //doc.Write(String.Format("<div style=\"{0}\">{1}</div><br />", style, str));
                doc.Write(String.Format("<div style=\"{0}\">{1}</div>", style, str));
                //doc.Body.ScrollTop = int.MaxValue;
                //CommunicationsDisplay.Document.Window.ScrollTo(0, int.MaxValue);
                line = 3;
                ScrollMessageIntoView();
                //System.Diagnostics.Debug.WriteLine("~~~~~~ OnCommunications 5");
                line = 4;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION IN OnCommunications @ Line: {line}, {ex.Message}");
            }
        }

        /// <summary>
        /// force the web control to the last item in the window... set to the bottom for the latest activity
        /// </summary>
        private void ScrollMessageIntoView()
        {
            // MOST IMP : processes all windows messages queue
            System.Windows.Forms.Application.DoEvents();

            if (CommunicationsDisplay.Document != null)
            {
                CommunicationsDisplay.Document.Window.ScrollTo(0, CommunicationsDisplay.Document.Body.ScrollRectangle.Height);
            }
        }

        private void ClearEventAndStatusDisplays()
        {
            // Clear communications
            displayReady = false;
            CommunicationsDisplay.Navigate("about:blank");
            while (!displayReady)
            {
                Application.DoEvents();
            }
        }

        private void CommunicationsDisplay_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //Debug.WriteLine("CommunicationsDisplay_Navigated");
            //OnCommunications("........", INK.CLR_BLACK);
            displayReady = true;
        }
        #endregion

        #region UNSAFE CODE
        // The unsafe keyword allows pointers to be used within the following method:
        static unsafe void Copy(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
        {
            try
            {
                if (src == null || srcIndex < 0 || dst == null || dstIndex < 0 || count < 0)
                {
                    Console.WriteLine("Serious Error in the Copy function 1");
                    throw new System.ArgumentException();
                }

                int srcLen = src.Length;
                int dstLen = dst.Length;
                if (srcLen - srcIndex < count || dstLen - dstIndex < count)
                {
                    Console.WriteLine("Serious Error in the Copy function 2");
                    throw new System.ArgumentException();
                }

                // The following fixed statement pins the location of the src and dst objects
                // in memory so that they will not be moved by garbage collection.
                fixed (byte* pSrc = src, pDst = dst)
                {
                    byte* ps = pSrc + srcIndex;
                    byte* pd = pDst + dstIndex;

                    // Loop over the count in blocks of 4 bytes, copying an integer (4 bytes) at a time:
                    for (int i = 0; i < count / 4; i++)
                    {
                        *((int*)pd) = *((int*)ps);
                        pd += 4;
                        ps += 4;
                    }

                    // Complete the copy by moving any bytes that weren't moved in blocks of 4:
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
                var exceptionMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                Debug.WriteLine("EXCEPTION IN: Copy - " + exceptionMessage);
            }

        }
        #endregion


        /*******************************************************/
        /// <summary>
        /// TCPiP server
        /// </summary>

        static AutoResetEvent autoEvent;//mutex
        static AutoResetEvent autoEvent2;//mutex
        /*******************************************************/
    }
}
