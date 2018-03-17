using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BlockAPP_Core.Core.Network
{
    public static class PacketFunctions
    {
        public static Byte[] StructureToByteArray(Object _Object)
        {
            Int32 _RawSize = Marshal.SizeOf(_Object);
            IntPtr _Buffer = Marshal.AllocHGlobal(_RawSize);
            Marshal.StructureToPtr(_Object, _Buffer, false);
            Byte[] _RawDatas = new Byte[_RawSize];
            Marshal.Copy(_Buffer, _RawDatas, 0, _RawSize);
            Marshal.FreeHGlobal(_Buffer);
            return _RawDatas;
        }

        public static Object ByteArrayToStructure(Byte[] _RawDatas, Type _Type)
        {
            Int32 _RawSize = Marshal.SizeOf(_Type);
            if (_RawSize > _RawDatas.Length)
                return null;

            IntPtr _Buffer = Marshal.AllocHGlobal(_RawSize);
            Marshal.Copy(_RawDatas, 0, _Buffer, _RawSize);
            Object _Object = Marshal.PtrToStructure(_Buffer, _Type);
            Marshal.FreeHGlobal(_Buffer);
            return _Object;
        }
    }
}
