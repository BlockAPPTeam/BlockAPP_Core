using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class Hashing
    {
        public static String GetHashForString(String _Data)
        {
            using (var _SHA512 = SHA512.Create())
            {
                var _Bytes = _SHA512.ComputeHash(Encoding.UTF8.GetBytes(_Data));
                return BitConverter.ToString(_Bytes);
            }
        }

        public static BigInteger GetId(String _Data)
        {
            using (var _SHA512 = SHA512.Create())
            {
                var _Bytes = _SHA512.ComputeHash(Encoding.UTF8.GetBytes(_Data));
                return new BigInteger(_Bytes);
            }
        }
    }
}
