using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class RSA
    {
        public static String SignData(String _Data, RSAParameters _PrivateKey)
        {
            Byte[] _SignedBytes;
            using (var _RSA = new RSACryptoServiceProvider())
            {
                Byte[] _OriginalData = Encoding.UTF8.GetBytes(_Data);

                try
                {
                    _RSA.ImportParameters(_PrivateKey);
                    _SignedBytes = _RSA.SignData(_OriginalData, CryptoConfig.MapNameToOID("SHA512"));
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
                finally
                {
                    _RSA.PersistKeyInCsp = false;
                }
            }
            return Convert.ToBase64String(_SignedBytes);
        }

        public static Boolean VerifyData(String _OriginalData, String _SignedMessage, RSAParameters _PublicKey)
        {
            Boolean _Success = false;
            using (var _RSA = new RSACryptoServiceProvider())
            {
                Byte[] _BytesToVerify = Encoding.UTF8.GetBytes(_OriginalData);
                Byte[] _SignedBytes = Convert.FromBase64String(_SignedMessage);
                try
                {
                    _RSA.ImportParameters(_PublicKey);
                    SHA512Managed Hash = new SHA512Managed();
                    Byte[] _HashedData = Hash.ComputeHash(_SignedBytes);

                    _Success = _RSA.VerifyData(_BytesToVerify, CryptoConfig.MapNameToOID("SHA512"), _SignedBytes);
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    _RSA.PersistKeyInCsp = false;
                }
            }
            return _Success;
        }
    }
}
