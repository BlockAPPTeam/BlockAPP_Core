using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Core.Network.Models
{
    public class MotherRawPackets
    {
        public MotherRawPackets(String _ClientId)
        {
            ClientId = _ClientId;
            RawPacketsList = new Queue<RawPackets>();
            Remainder = new byte[1024];

            BytesRemaining = 0;
        }


        public void AddToList(Byte[] _Data, int _SizeOfChunk)
        {
            lock (RawPacketsList)
            {
                RawPacketsList.Enqueue(new RawPackets(ClientId, _Data, _SizeOfChunk));
            }
        }
        public void ClearList()
        {
            lock (RawPacketsList)
            {
                RawPacketsList.Clear();
            }
        }

        public RawPackets GetTopItem
        {
            get
            {
                RawPackets _RawPacket;
                lock (RawPacketsList)
                {
                    _RawPacket = RawPacketsList.Dequeue();
                }
                return _RawPacket;
            }
        }

        public int GetItemCount
        {
            get { return RawPacketsList.Count; }
        }

        public String ClientId { get; private set; }
        public int BytesRemaining { get; set; }
        public Byte[] Remainder { get; set; }

        private Queue<RawPackets> RawPacketsList;
    }
}
