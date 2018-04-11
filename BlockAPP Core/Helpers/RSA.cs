using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class RSA
    {
        public static Models.KeyPair GenerateKeyPair()
        {
            RsaKeyPairGenerator _RSAKeyPairGnr = new RsaKeyPairGenerator();
            _RSAKeyPairGnr.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            AsymmetricCipherKeyPair _KeyPair = _RSAKeyPairGnr.GenerateKeyPair();

            PrivateKeyInfo _PrivateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(_KeyPair.Private);
            Byte[] _SerializedPrivateBytes = _PrivateKeyInfo.ToAsn1Object().GetDerEncoded();
            String _SerializedPrivate = Convert.ToBase64String(_SerializedPrivateBytes);

            SubjectPublicKeyInfo _PublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(_KeyPair.Public);
            Byte[] _SerializedPublicBytes = _PublicKeyInfo.ToAsn1Object().GetDerEncoded();
            String _SerializedPublic = Convert.ToBase64String(_SerializedPublicBytes);

            if (Db.DbContextManager._Db.Accounts.Any(x => x.PublicKey == _SerializedPublic))
            {
                return GenerateKeyPair();
            }

            Models.KeyPair _KP = new Models.KeyPair
            {
                PriveteKey = _SerializedPrivate,
                PublicKey = _SerializedPublic
            };
            return _KP;
        }

        public static String Sign(String _Data, String _SerializedKey)
        {
            RsaPrivateCrtKeyParameters _Key = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(_SerializedKey));
            ISigner _Signer = SignerUtilities.GetSigner("SHA1withRSA");
            _Signer.Init(true, _Key);

            var _DataBytes = Encoding.UTF8.GetBytes(_Data);
            _Signer.BlockUpdate(_DataBytes, 0, _DataBytes.Length);
            Byte[] _Signature = _Signer.GenerateSignature();

            var _SignedString = Convert.ToBase64String(_Signature);
            return _SignedString;
        }

        public static Boolean Verify(String _Data, String _Signature, String _SerializedKey)
        {
            RsaKeyParameters _PublicKey = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(_SerializedKey));
            ISigner _Signer = SignerUtilities.GetSigner("SHA1withRSA");
            _Signer.Init(false, _PublicKey);
            
            var _DataBytes = Encoding.UTF8.GetBytes(_Data);
            _Signer.BlockUpdate(_DataBytes, 0, _DataBytes.Length);
            var _ExpectedSignature = Convert.FromBase64String(_Signature);
            return _Signer.VerifySignature(_ExpectedSignature);
        }
    }
}
