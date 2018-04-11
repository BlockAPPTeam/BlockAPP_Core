using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace BlockAPP_Core.Core.Network
{
    class ServerPacket
    {
        public Socket CurrentSocket;
        public String Guid;
        public Byte[] DataBuffer = new byte[32768];
      
        public ServerPacket(Socket _Sock, String _Guid)
        {
            CurrentSocket = _Sock;
            Guid = _Guid;
        }
    }
}
