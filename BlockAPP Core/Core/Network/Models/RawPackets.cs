using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Core.Network.Models
{
    public class RawPackets
    {
        public RawPackets(String _ClientId, Byte[] _DataChunk, int _DataLength)
        {
            DataChunk = new byte[_DataLength];
            DataChunk = _DataChunk;
            ClientId = _ClientId;
            DataLength = _DataLength;
        }

        public Byte[] DataChunk { get; set; }
        public String ClientId { get; set; }
        public int DataLength { get; set; }
    }
}
