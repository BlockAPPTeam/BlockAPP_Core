using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BlockAPP_Core.Core.Network.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PacketData
    {

        /****************************************************************/
        //HEADER is 6 BYTES
        public UInt16 Packet_Type;  //TYPE_??
        public UInt16 Packet_Size;
        public UInt16 Packet_SubType;
        /****************************************************************/

        public UInt32 Timestamp;

        [JsonIgnore]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 300)]
        public Char[] Signature = new Char[300];
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 300)]
        public Char[] PublicKey = new Char[300];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 618)]
        public Char[] Data = new Char[618];

        //300 + 300 + 618 + 6 = 1024

    }
}
