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
            StationName = String.Empty;
            ClientName = String.Empty;
            UserListentingPort = 9998;//default
            AlternateIP = String.Empty;
        }

        public String Id { get; private set; } 
        public Socket UserSocket { get; private set; }
        public String ClientName { get; set; }
        public String StationName { get; set; }
        public UInt16 UserListentingPort { get; set; }
        public String AlternateIP { get; set; }


        public int _ZeroDataCount { get; internal set; }
    }
}
