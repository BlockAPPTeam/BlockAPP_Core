using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Core.Network.Models
{
    public class FullPacket
    {
        public FullPacket(String _ClientId, Byte[] _Data)
        {
            Data = new byte[1024];
            Data = _Data;
            ClientId = _ClientId;
        }

        public Byte[] Data { get; set; }
        public String ClientId { get; set; }
    }
}
