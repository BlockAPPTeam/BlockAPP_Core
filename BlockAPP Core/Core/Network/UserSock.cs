using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace BlockAPP_Core.Core.Network
{
    public class UserSock
    {
        public UserSock(String _Guid, Socket _Sock)
        {
            Id = _Guid;
            UserSocket = _Sock;
            MessageBuilder = new List<byte>();
        }

        public String Id { get; private set; } 
        public Socket UserSocket { get; private set; }

        public Boolean Accepted { get; set; }
        public String PublicKey { get; set; }
        public String Signature { get; set; }
        public long Timestamp { get; set; }


        public int _ZeroDataCount { get; internal set; }

        public List<Byte> MessageBuilder { get; set; }
    }
}
