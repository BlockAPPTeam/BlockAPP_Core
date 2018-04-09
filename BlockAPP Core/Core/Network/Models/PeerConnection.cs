using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace BlockAPP_Core.Core.Network.Models
{
    public class PeerConnection
    {
        public Socket PeerSocket {get; set; }
        public String PublickKey { get; set; }
        public Boolean Auth { get; set; }
    }
}
