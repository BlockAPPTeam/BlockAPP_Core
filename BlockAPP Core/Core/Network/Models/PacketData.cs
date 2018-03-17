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

            public UInt32 Data1;        //miscellanious information
            public UInt32 Data2;        //miscellanious information
            public UInt32 Data3;        //miscellanious information
            public UInt32 Data4;        //miscellanious information
            public UInt32 Data5;        //miscellanious information

            public Int32 Data6;        //miscellanious information
            public Int32 Data7;        //miscellanious information
            public Int32 Data8;        //miscellanious information
            public Int32 Data9;        //miscellanious information
            public Int32 Data10;       //miscellanious information

            public UInt32 Data11;        //miscellanious information
            public UInt32 Data12;        //miscellanious information
            public UInt32 Data13;        //miscellanious information
            public UInt32 Data14;        //miscellanious information
            public UInt32 Data15;        //miscellanious information

            public Int32 Data16;        //miscellanious information
            public Int32 Data17;        //miscellanious information
            public Int32 Data18;        //miscellanious information
            public Int32 Data19;        //miscellanious information
            public Int32 Data20;       //miscellanious information

            public UInt32 Data21;        //miscellanious information
            public UInt32 Data22;        //miscellanious information
            public UInt32 Data23;        //miscellanious information
            public UInt32 Data24;        //miscellanious information
            public UInt32 Data25;        //miscellanious information

            public Int32 Data26;        //miscellanious information
            public Int32 Data27;        //miscellanious information
            public Int32 Data28;        //miscellanious information
            public Int32 Data29;        //miscellanious information
            public Int32 Data30;       //miscellanious information

            public Double DataDouble1;
            public Double DataDouble2;
            public Double DataDouble3;
            public Double DataDouble4;
            public Double DataDouble5;

            /// <summary>
            /// Long value1
            /// </summary>
            public Int64 DataLong1;
            /// <summary>
            /// Long value2
            /// </summary>
            public Int64 DataLong2;
            /// <summary>
            /// Long value3
            /// </summary>
            public Int64 DataLong3;
            /// <summary>
            /// Long value4
            /// </summary>
            public Int64 DataLong4;

            /// <summary>
            /// Unsigned Long value1
            /// </summary>
            public UInt64 DataULong1;
            /// <summary>
            /// Unsigned Long value2
            /// </summary>
            public UInt64 DataULong2;
            /// <summary>
            /// Unsigned Long value3
            /// </summary>
            public UInt64 DataULong3;
            /// <summary>
            /// Unsigned Long value4
            /// </summary>
            public UInt64 DataULong4;

            /// <summary>
            /// DateTime Tick value1
            /// </summary>
            public Int64 DataTimeTick1;

            /// <summary>
            /// DateTime Tick value2
            /// </summary>
            public Int64 DataTimeTick2;

            /// <summary>
            /// DateTime Tick value1
            /// </summary>
            public Int64 DataTimeTick3;

            /// <summary>
            /// DateTime Tick value2
            /// </summary>
            public Int64 DataTimeTick4;

            /// <summary>
            /// 300 Chars
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 300)]
            public Char[] szStringDataA = new Char[300];

            /// <summary>
            /// 300 Chars
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 300)]
            public Char[] szStringDataB = new Char[300];

            /// <summary>
            /// 150 Chars
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 150)]
            public Char[] szStringData150 = new Char[150];

            //18 + 120 + 40 + 96 + 600 + 150 = 1024
        
    }
}
