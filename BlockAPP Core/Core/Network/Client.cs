using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BlockAPP_Core.Core.Network
{
    public class Client
    {
        public delegate void ReceiveDataCallback(byte[] message, int messageSize);
        public delegate void ReceiveBroadcastCallback(byte[] message, int messageSize);
        public delegate void DisconnectCallback();

        private ReceiveDataCallback _receive = null;
        private ReceiveBroadcastCallback _broadcast = null;
        private DisconnectCallback _disconnect = null;

        private Socket _clientSocket;
        private Socket _broadcastSocket = null;
        private bool _receiveBroadcasts = false;
        private int _broadcastPort = 0;
        public DateTime LastDataFromServer;

        public ReceiveDataCallback OnReceiveData
        {
            get
            {
                return _receive;
            }

            set
            {
                _receive = value;
            }
        }
        public ReceiveBroadcastCallback OnReceiveBroadcast
        {
            get
            {
                return _broadcast;
            }

            set
            {
                _broadcast = value;
            }
        }
        public DisconnectCallback OnDisconnected
        {
            get
            {
                return _disconnect;
            }

            set
            {
                _disconnect = value;
            }
        }

        public bool Connected
        {
            get
            {
                if (_clientSocket == null)
                    return false;
                else
                    return _clientSocket.Connected;
            }
        }
        public bool ReceiveBroadcasts
        {
            get
            {
                return _receiveBroadcasts;
            }

            set
            {
                if (_receiveBroadcasts != value)
                {
                    _receiveBroadcasts = value;
                    if (_receiveBroadcasts)
                    {
                        if (_broadcastPort > 0)
                            SetupBroadcastSocket();
                    }

                    else if (_broadcastSocket != null)
                        _broadcastSocket.Close();
                }
            }
        }
        public int BroadcastPort
        {
            get
            {
                return _broadcastPort;
            }

            set
            {
                if (_broadcastPort != value)
                {
                    _broadcastPort = value;

                    if (_receiveBroadcasts)
                        SetupBroadcastSocket();
                }
            }
        }

        public Client()
        {
            LastDataFromServer = DateTime.Now;
        }

        public void Connect(IPAddress address, int port)
        {
            try
            {
                Disconnect();

                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(new IPEndPoint(address, port));

                if (_clientSocket.Connected)
                    WaitForData();
            }

            catch (SocketException se)
            {
                // ToDo
            }
        }
        public void Disconnect()
        {
            if (_clientSocket != null)
                _clientSocket.Close();
        }

        public void SendMessage(byte[] message)
        {
            if (_clientSocket != null)
                if (_clientSocket.Connected)
                    _clientSocket.Send(message);
        }

        private void WaitForData()
        {
            try
            {
                Packet pack = new Packet(_clientSocket);
                _clientSocket.BeginReceive(pack.DataBuffer, 0, pack.DataBuffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), pack);
            }

            catch (SocketException se)
            {
                // ToDo
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                Packet socketData = (Packet)asyn.AsyncState;
                int dataSize = socketData.CurrentSocket.EndReceive(asyn);

                if (_receive != null)
                    _receive(socketData.DataBuffer, dataSize);

                WaitForData();
            }

            catch (ObjectDisposedException)
            {
                System.Console.WriteLine("Client EXCEPTION in OnDataReceived: Socket has been closed");
            }

            catch (SocketException se)
            {
                System.Console.WriteLine("Client EXCEPTION in OnDataReceived: " + se.Message);

                if (OnDisconnected != null)
                    OnDisconnected();
                //ToDo
            }
        }

        private void SetupBroadcastSocket()
        {
            if (_broadcastSocket != null)
                _broadcastSocket.Close();

            _broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _broadcastSocket.Bind((EndPoint)(new IPEndPoint(IPAddress.Any, _broadcastPort)));

            WaitForBroadcast();
        }
        
        private void WaitForBroadcast()
        {
            try
            {
                Packet pack = new Packet(_broadcastSocket);
                EndPoint port = (EndPoint)(new IPEndPoint(IPAddress.Any, _broadcastPort));

                _broadcastSocket.BeginReceiveFrom(pack.DataBuffer, 0, pack.DataBuffer.Length, SocketFlags.None, ref port, new AsyncCallback(OnBroadcastReceived), pack);
            }

            catch (SocketException se)
            {
                System.Console.WriteLine("Client EXCEPTION in WaitForBroadcast: " + se.Message);
            }
        }

        private void OnBroadcastReceived(IAsyncResult asyn)
        {
            try
            {
                Packet socketData = (Packet)asyn.AsyncState;
                int dataSize = socketData.CurrentSocket.EndReceive(asyn);

                if (_broadcast != null)
                    _broadcast(socketData.DataBuffer, dataSize);

                WaitForBroadcast();
            }

            catch (ObjectDisposedException)
            {
                System.Console.WriteLine("Client EXCEPTION in OnBroadcastReceived: Socket has been closed");
            }

            catch (SocketException se)
            {
                System.Console.WriteLine("Client EXCEPTION in OnBroadcastReceived: " + se.Message);
            }
        }
        
        private class Packet
        {
            public Socket CurrentSocket;
            public byte[] DataBuffer = new byte[1024];

            public Packet(Socket sock)
            {
                CurrentSocket = sock;
            }
        }
    }
}
