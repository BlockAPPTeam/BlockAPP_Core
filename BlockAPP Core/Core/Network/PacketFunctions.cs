using BlockAPP_Core.Helpers;
using Newtonsoft.Json;
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

        public static Boolean Verify(this Models.PacketData _Packet)
        {
            var _Signature = NormalizeChars(_Packet.Signature);
            var _PublicKey = NormalizeChars(_Packet.PublicKey);

            var _BlockJSON = JsonConvert.SerializeObject(_Packet);
            var _BlockHash = Hashing.GetHashForString(_BlockJSON);
            return RSA.Verify(_BlockHash, _Signature, _PublicKey);
        }

        public static Models.PacketData SignPacket(this Models.PacketData _Packet, String _Key)
        {
            var _Timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            var _BlockJSON = JsonConvert.SerializeObject(_Packet);
            var _BlockHash = Hashing.GetHashForString(_BlockJSON);
            _Packet.Signature = RSA.Sign(_BlockHash, _Key).ToCharArray();

            return _Packet;
        }

        public static String NormalizeChars(this Char[] _Data)
        {
            return new string(_Data).TrimEnd('\0');
        }

        public static DateTime UnixTimeStampToDateTime(long _Timestamp)
        {
            DateTime _DateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            _DateTime = _DateTime.AddSeconds(_Timestamp).ToUniversalTime();
            return _DateTime;
        }
    }
}
