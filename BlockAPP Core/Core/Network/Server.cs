using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BlockAPP_Core.Core.Network
{
    public class Server
    {
        public delegate void ClientConnectCallback(String Guid);
        public delegate void ClientDisconnectCallback(String Guid);

        public delegate void ReceiveDataCallback(String Guid, byte[] message, int messageSize);

        private ClientConnectCallback _ClientConnect = null;
        private ClientDisconnectCallback _ClientDisconnect = null;
        private ReceiveDataCallback _Receive = null;

        private Socket _MainSocket;
        public Dictionary<String, UserSock> _WorkerSockets = new Dictionary<String, UserSock>();


        public ClientConnectCallback OnClientConnect
        {
            get
            {
                return _ClientConnect;
            }

            set
            {
                _ClientConnect = value;
            }
        }
        public ClientDisconnectCallback OnClientDisconnect
        {
            get
            {
                return _ClientDisconnect;
            }

            set
            {
                _ClientDisconnect = value;
            }
        }
        public ReceiveDataCallback OnReceiveData
        {
            get
            {
                return _Receive;
            }

            set
            {
                _Receive = value;
            }
        }

        public Boolean IsListening
        {
            get
            {
                if (_MainSocket == null)
                    return false;
                else
                    return _MainSocket.IsBound;
            }
        }


        public void Start(int _Port)
        {
            try
            {
                Stop();

                _MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _MainSocket.Bind(new IPEndPoint(IPAddress.Any, _Port));
                _MainSocket.Listen(100);
                _MainSocket.BeginAccept(new AsyncCallback(OnReceiveConnection), null);
            }
            catch (SocketException ex)
            {
                // ToDo
            }
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

            if (IsListening) _MainSocket.Close();
        }


        public void SendMessageToAll(Byte[] _Data)
        {
            List<String> _ClientsToRemove = new List<String>();
            foreach (var _Guid in _WorkerSockets.Keys)
            {
                if (_WorkerSockets[_Guid].UserSocket.Connected)
                {
                    try
                    {
                        _WorkerSockets[_Guid].UserSocket.Send(_Data);
                    }
                    catch
                    {
                        _ClientsToRemove.Add(_Guid);
                    }

                    Thread.Sleep(10);// this is for a client Ping so stagger the send messages
                }
                else
                {
                    _ClientsToRemove.Add(_Guid);
                }
            }

            if (_ClientsToRemove.Any())
            {
                foreach (var _Guid in _ClientsToRemove)
                {
                    OnClientDisconnect(_Guid);
                }
            }
        }
        public void SendMessage(String _Guid, Byte[] _Data)
        {
            try
            {
                if (_WorkerSockets.ContainsKey(_Guid))
                {
                    _WorkerSockets[_Guid].UserSocket.Send(_Data);
                }
            }
            catch (SocketException ex)
            {
                // ToDo
            }
        }


        private void OnReceiveConnection(IAsyncResult _Asunc)
        {
            var _NewId = Guid.NewGuid().ToString();
            try
            {
                lock (_WorkerSockets)
                {
                    UserSock _US = new UserSock(_NewId, _MainSocket.EndAccept(_Asunc));
                    _WorkerSockets.Add(_NewId, _US);
                }

                _ClientConnect(_NewId);

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

                OnClientDisconnect(_NewId);
            }
        }
        private void WaitForData(String _Guid)
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
                    OnClientDisconnect(_Guid);
                }
                catch { }
            }
            catch (Exception ex)
            {
                OnClientDisconnect(_Guid);
            }
        }
        private void OnDataReceived(IAsyncResult _Asunc)
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
                            OnClientDisconnect(_SocketData.Guid);
                        }
                    }
                }
                else
                {
                    _Receive(_SocketData.Guid, _SocketData.DataBuffer, _DataSize);

                    _WorkerSockets[_SocketData.Guid]._ZeroDataCount = 0;
                }

                WaitForData(_SocketData.Guid);
            }
            catch (ObjectDisposedException)
            {
                OnClientDisconnect(_SocketData.Guid);
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

                    OnClientDisconnect(_SocketData.Guid);
                }
                else
                {
                    //string mess = "CONNECTION BOOTED for reason other than 10054: code = " + se.ErrorCode.ToString() + ",   " + se.Message;
                    // ToDo
                }
            }
        }
    }
}
