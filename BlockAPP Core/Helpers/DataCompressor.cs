using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class DataCompressor
    {
        private static void CopyTo(Stream _Stream, Stream _Destination)
        {
            Byte[] _Bytes = new Byte[4096];
            int cnt;
            while ((cnt = _Stream.Read(_Bytes, 0, _Bytes.Length)) != 0)
            {
                _Destination.Write(_Bytes, 0, cnt);
            }
        }

        public static Byte[] Zip(String _Data)
        {
            var _Bytes = Encoding.UTF8.GetBytes(_Data);

            using (var _MSI = new MemoryStream(_Bytes))
            using (var _MSO = new MemoryStream())
            {
                using (var _GS = new GZipStream(_MSO, CompressionMode.Compress))
                {
                    CopyTo(_MSI, _GS);
                }

                return _MSO.ToArray();
            }
        }

        public static string Unzip(Byte[] bytes)
        {
            using (var _MSI = new MemoryStream(bytes))
            using (var _MSO = new MemoryStream())
            {
                using (var _GS = new GZipStream(_MSI, CompressionMode.Decompress))
                {
                    CopyTo(_GS, _MSO);
                }

                return Encoding.UTF8.GetString(_MSO.ToArray());
            }
        }
    }
}
