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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 344)]
        public Char[] Signature = new Char[344];
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 392)]
        public Char[] PublicKey = new Char[392];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32022)]
        public Byte[] Data = new Byte[32022];

        //344 + 392 + 6 + 4 + 32022 = 32768

    }
}
