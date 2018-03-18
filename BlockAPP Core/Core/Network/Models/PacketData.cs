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
        //HEADER is 18 BYTES
        public UInt16 Packet_Type;  //TYPE_??
        public UInt16 Packet_Size;
        public UInt16 Data_Type;    // DATA_ type fields
        public UInt16 maskTo;       // SENDTO_MY_SHUBONLY and the like.
        public UInt32 idTo;         // Used if maskTo is SENDTO_INDIVIDUAL
        public UInt32 idFrom;       // Client ID value
        public UInt16 nAppLevel;
        /****************************************************************/

        public UInt32 Timestamp;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 300)]
        public Char[] Signature = new Char[300];
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 300)]
        public Char[] PublicKey = new Char[300];

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 624)]
        public Char[] Data = new Char[624];

        //300 + 300 + 624 = 1024

    }
}
