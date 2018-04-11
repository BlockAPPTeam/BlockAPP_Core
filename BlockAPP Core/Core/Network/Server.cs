using BlockAPP_Core.Core.Network.Enums;
using BlockAPP_Core.Core.Network.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BlockAPP_Core.Core.Network
{
    public class Server
    {
        private Socket _MainSocket;

        Dictionary<String, UserSock> _WorkerSockets = new Dictionary<String, UserSock>();
        Dictionary<String, MotherRawPackets> _ClientsRawPackets = new Dictionary<string, MotherRawPackets>();
        Queue<FullPacket> _FullPackets = new Queue<FullPacket>();

        Timer CleanUpTimer;
        Thread DataProcessThread = null;
        Thread FullPacketDataProcessThread = null;

        Boolean _Close = false;
        AutoResetEvent autoEvent;//mutex
        AutoResetEvent autoEvent2;//mutex

        public Server(int _Port)
        {
            autoEvent = new AutoResetEvent(false); //the RawPacket data mutex
            autoEvent2 = new AutoResetEvent(false);//the FullPacket data mutex
            DataProcessThread = new Thread(NormalizeThePackets);
            FullPacketDataProcessThread = new Thread(ProcessRecievedData);

            _MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _MainSocket.Bind(new IPEndPoint(IPAddress.Any, _Port));
            _MainSocket.Listen(100);
            _MainSocket.BeginAccept(new AsyncCallback(OnReceiveConnection), null);

            DataProcessThread.Start();
            FullPacketDataProcessThread.Start();

            CleanUpTimer = new Timer(CleanConnections);
            CleanUpTimer.Change(0, 30000);
        }
        public void Stop()
        {
            lock (_WorkerSockets)
            {
                foreach (UserSock _US in _WorkerSockets.Values)
                {
                    if (_US.UserSocket.Connected) _US.UserSocket.Close();
                }
                _WorkerSockets.Clear();
            }

            if (_MainSocket.IsBound) _MainSocket.Close();

            CleanUpTimer.Dispose();
        }
        

        public void Connect(IPAddress address, int port)
        {
            var _NewId = Guid.NewGuid().ToString();
            try
            {
                var _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(new IPEndPoint(address, port));
                lock (_WorkerSockets)
                {
                    UserSock _US = new UserSock(_NewId, _clientSocket);
                    _WorkerSockets.Add(_NewId, _US);
                }


                NewClientConnected(_NewId);

                WaitForData(_NewId);
            }
            catch (ObjectDisposedException)
            {
                // ToDo
            }
        }

        public void SendMessage(String _Guid, Byte[] _Data, PacketType _Type)
        {
            try
            {
                if (_WorkerSockets.ContainsKey(_Guid))
                {
                    Console.WriteLine($"Server {_MainSocket.LocalEndPoint.ToString()}: Send message {_Type.ToString()} to {_Guid}");
                    _WorkerSockets[_Guid].UserSocket.Send(_Data);
                }
            }
            catch (SocketException ex)
            {
                // ToDo
            }
        }




        StringBuilder _SB;
        void AssembleMessage(String _ClientID, Byte[] _Data)
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
                            if (_WorkerSockets.ContainsKey(_ClientID))
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

                            SendMessage(_ClientID, _ResponceData, PacketType.TYPE_MessageReceived);
                        }
                        break;
                }
            }
            catch
            {
                // ToDo
            }
        }

        #region Server Core
        void NormalizeThePackets()
        {
            while (_MainSocket.IsBound)
            {
                autoEvent.WaitOne(10000);

                lock (_ClientsRawPackets)
                {
                    foreach (var _MRawPacket in _ClientsRawPackets.Values)
                    {
                        if (_MRawPacket.GetItemCount == 0) continue;

                        try
                        {
                            Byte[] _PacketStorage = new byte[360448];//good for 10 full packets + 1 remainder

                            RawPackets _NewRawPacket;

                            while (true)
                            {
                                if (_MRawPacket.GetItemCount == 0)
                                    break;

                                int _HoldLength = 0;

                                if (_MRawPacket.BytesRemaining > 0)
                                {
                                    Helpers.UnsafeMethods.Copy(_MRawPacket.Remainder, 0, _PacketStorage, 0, _MRawPacket.BytesRemaining);
                                }
                                _HoldLength = _MRawPacket.BytesRemaining;

                                for (int i = 0; i < 10; i++)
                                {
                                    _NewRawPacket = _MRawPacket.GetTopItem;
                                    Helpers.UnsafeMethods.Copy(_NewRawPacket.DataChunk, 0, _PacketStorage, _HoldLength, _NewRawPacket.DataLength);
                                    _HoldLength += _NewRawPacket.DataLength;

                                    if (_MRawPacket.GetItemCount == 0)
                                    {
                                        break;
                                    }
                                }

                                int _ActualPackets = 0;

                                #region PACKET_SIZE 32768
                                if (_HoldLength >= 32768)//make sure we have at least one packet in there
                                {
                                    _ActualPackets = _HoldLength / 32768;
                                    _MRawPacket.BytesRemaining = _HoldLength - (_ActualPackets * 32768);

                                    for (int i = 0; i < _ActualPackets; i++)
                                    {
                                        Byte[] _Temp = new byte[32768];
                                        Helpers.UnsafeMethods.Copy(_PacketStorage, i * 32768, _Temp, 0, 32768);
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

                                Helpers.UnsafeMethods.Copy(_PacketStorage, _ActualPackets * 32768, _MRawPacket.Remainder, 0, _MRawPacket.BytesRemaining);
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
                            // ToDo
                        }
                    }
                }

                if (_Close) break;
            }
        }
        void NewClientConnected(String _CLientId)
        {
            try
            {
                if (_WorkerSockets.ContainsKey(_CLientId))
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
        void OnReceiveConnection(IAsyncResult _Asunc)
        {
            var _NewId = Guid.NewGuid().ToString();
            try
            {
                lock (_WorkerSockets)
                {
                    UserSock _US = new UserSock(_NewId, _MainSocket.EndAccept(_Asunc));
                    _WorkerSockets.Add(_NewId, _US);
                }


                NewClientConnected(_NewId);

                WaitForData(_NewId);
                _MainSocket.BeginAccept(new AsyncCallback(OnReceiveConnection), null);
            }
            catch (ObjectDisposedException)
            {
                // ToDo
            }
            catch (SocketException ex)
            {
                // ToDo

                if (_WorkerSockets.ContainsKey(_NewId))
                {
                    //Console.WriteLine("RemoteEndPoint: " + _WorkerSockets[_NewId].UserSocket.RemoteEndPoint.ToString());
                    //Console.WriteLine("LocalEndPoint: " + _WorkerSockets[_NewId].UserSocket.LocalEndPoint.ToString());

                    //Console.WriteLine("Closing socket from OnReceiveConnection");
                    // ToDo
                }

                ClientDisconnect(_NewId);
            }
        }
        void WaitForData(String _Guid)
        {
            if (!_WorkerSockets.ContainsKey(_Guid))
            {
                return;
            }

            try
            {
                ServerPacket _Packet = new ServerPacket(_WorkerSockets[_Guid].UserSocket, _Guid);
                _WorkerSockets[_Guid].UserSocket.BeginReceive(_Packet.DataBuffer, 0, _Packet.DataBuffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), _Packet);
            }
            catch (SocketException se)
            {
                try
                {
                    ClientDisconnect(_Guid);
                }
                catch { }
            }
            catch (Exception ex)
            {
                ClientDisconnect(_Guid);
            }
        }
        void OnDataReceived(IAsyncResult _Asunc)
        {
            ServerPacket _SocketData = (ServerPacket)_Asunc.AsyncState;

            try
            {
                int _DataSize = _SocketData.CurrentSocket.EndReceive(_Asunc);

                if (_DataSize.Equals(0))
                {
                    if (_WorkerSockets.ContainsKey(_SocketData.Guid))
                    {
                        if (((UserSock)_WorkerSockets[_SocketData.Guid])._ZeroDataCount++ == 10)
                        {
                            ClientDisconnect(_SocketData.Guid);
                        }
                    }
                }
                else
                {
                    if (_ClientsRawPackets.ContainsKey(_SocketData.Guid))
                    {
                        _ClientsRawPackets[_SocketData.Guid].AddToList(_SocketData.DataBuffer, _DataSize);

                        autoEvent.Set();
                    }

                    _WorkerSockets[_SocketData.Guid]._ZeroDataCount = 0;
                }

                WaitForData(_SocketData.Guid);
            }
            catch (ObjectDisposedException)
            {
                ClientDisconnect(_SocketData.Guid);
            }
            catch (SocketException se)
            {
                //10060 - A connection attempt failed because the connected party did not properly respond after a period of time,
                //or established connection failed because connected host has failed to respond.
                if (se.ErrorCode == 10054 || se.ErrorCode == 10060) //10054 - Error code for Connection reset by peer
                {
                    try
                    {
                        //System.Diagnostics.Debug.WriteLine("SERVER EXCEPTION in OnClientDataReceived, ServerObject removed:(" + se.ErrorCode.ToString() + ") " + _SocketData.Guid + ", (happens during a normal client exit)");
                        //System.Diagnostics.Debug.WriteLine("RemoteEndPoint: " + _WorkerSockets[_SocketData.Guid].UserSocket.RemoteEndPoint.ToString());
                        //System.Diagnostics.Debug.WriteLine("LocalEndPoint: " + _WorkerSockets[_SocketData.Guid].UserSocket.LocalEndPoint.ToString());
                        // ToDo
                    }
                    catch { }

                    ClientDisconnect(_SocketData.Guid);
                }
                else
                {
                    //string mess = "CONNECTION BOOTED for reason other than 10054: code = " + se.ErrorCode.ToString() + ",   " + se.Message;
                    // ToDo
                }
            }
        }

        void ProcessRecievedData()
        {
            while (_MainSocket.IsBound)
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
                                    CheckUserCredentials(_NewFullPacket.ClientId, _NewFullPacket.Data);
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
                            case (UInt16)PacketType.TYPE_RequestCredentials:
                                {
                                    SendCredentials(_NewFullPacket.ClientId);
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
        #endregion

        #region System methods

        void SetNewConnectionData_FromThread(String _CLientId)
        {
            try
            {
                if (_WorkerSockets[_CLientId].UserSocket.Connected)
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
        void ClientDisconnect(String _CLientId)
        {
            if (_Close) return;

            /*******************************************************/
            lock (_ClientsRawPackets)//Make sure we don't do this twice
            {
                if (!_ClientsRawPackets.ContainsKey(_CLientId))
                {
                    lock (_WorkerSockets)
                    {
                        if (!_WorkerSockets.ContainsKey(_CLientId))
                        {
                            return;
                        }
                    }
                }
            }
            /*******************************************************/

            try
            {
                SendMessageOfClientDisconnect(_CLientId);
            }
            catch (Exception ex)
            {
                // ToDo
            }

            CleanupDeadClient(_CLientId);


            Thread.Sleep(10);
        }
        void CleanupDeadClient(String _Guid)
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
                lock (_WorkerSockets)
                {
                    if (_WorkerSockets.ContainsKey(_Guid))
                    {
                        _WorkerSockets[_Guid].UserSocket.Close();
                        _WorkerSockets.Remove(_Guid);
                    }
                }
            }
            catch { }
        }
        void CheckUserCredentials(String ClientId, Byte[] _Data)
        {
            try
            {
                PacketData IncomingData = new PacketData();
                IncomingData = (PacketData)PacketFunctions.ByteArrayToStructure(_Data, typeof(PacketData));

                lock (_WorkerSockets)
                {
                    if (PacketFunctions.Verify(IncomingData))
                    {
                        _WorkerSockets[ClientId].Timestamp = IncomingData.Timestamp;
                        _WorkerSockets[ClientId].Accepted = true;
                        _WorkerSockets[ClientId].Signature = IncomingData.Signature.NormalizeChars();
                        _WorkerSockets[ClientId].PublicKey = IncomingData.PublicKey.NormalizeChars();

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
        void CleanConnections(Object state)
        {
            try
            {
                List<String> ClientIDsToClear = new List<String>();

                lock (_WorkerSockets)
                {
                    foreach (var _Sock in _WorkerSockets.Values)
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
            catch { }
        }

        #endregion

        #region System Message send method

        void RequestNewConnectionCredentials(String _ClientId)
        {
            try
            {
                PacketData xdata = new PacketData();

                xdata.Packet_Type = (UInt16)PacketType.TYPE_RequestCredentials;
                xdata.Packet_Size = (UInt16)Marshal.SizeOf(typeof(PacketData));

                if (!_WorkerSockets.ContainsKey(_ClientId)) return;

                lock (_WorkerSockets)
                {
                    String _ClientAddr = ((IPEndPoint)_WorkerSockets[_ClientId].UserSocket.RemoteEndPoint).Address.ToString();
                    _ClientAddr.CopyTo(0, xdata.Data, 0, _ClientAddr.Length);

                    Byte[] _Data = PacketFunctions.StructureToByteArray(xdata);

                    if (_WorkerSockets[_ClientId].UserSocket.Connected)
                    {
                        SendMessage(_ClientId, _Data, PacketType.TYPE_RequestCredentials);
                    }
                }
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }
        void SendCredentials(String ClientId)
        {
            try
            {
                PacketData xdata = new PacketData();

                xdata.Packet_Type = (UInt16)PacketType.TYPE_MyCredentials;
                xdata.Packet_Size = (UInt16)Marshal.SizeOf(typeof(PacketData));

                xdata = xdata.SignPacket(SoftConfigs._LocalConfig.PrivateKey);

                Byte[] _Data = PacketFunctions.StructureToByteArray(xdata);
                SendMessage(ClientId, _Data, PacketType.TYPE_MyCredentials);
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }
        void SendRegisteredMessage(String ClientId)
        {
            try
            {
                PacketData xdata = new PacketData();

                xdata.Packet_Type = (UInt16)PacketType.TYPE_Registered;
                xdata.Packet_Size = (UInt16)Marshal.SizeOf(typeof(PacketData));

                xdata = xdata.SignPacket(SoftConfigs._LocalConfig.PrivateKey);

                Byte[] _Data = PacketFunctions.StructureToByteArray(xdata);
                SendMessage(ClientId, _Data, PacketType.TYPE_Registered);
            }
            catch { }
        }
        void SendMessageOfClientDisconnect(String _ClientId)
        {
            try
            {
                PacketData xdata = new PacketData();

                xdata.Packet_Type = (UInt16)PacketType.TYPE_ClientDisconnecting;
                xdata.Packet_Size = (UInt16)Marshal.SizeOf(typeof(PacketData));

                Byte[] _Data = PacketFunctions.StructureToByteArray(xdata);
                SendMessage(_ClientId, _Data, PacketType.TYPE_ClientDisconnecting);
            }
            catch (Exception ex)
            {
                // ToDo
            }
        }

        #endregion
    }
}
