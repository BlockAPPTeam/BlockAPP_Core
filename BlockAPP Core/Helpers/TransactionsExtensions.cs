using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class TransactionsExtensions
    {
        public static Int64 Calc(this Models.Transaction _Transaction, UInt64 _Height)
        {
            return (Int64)Math.Floor((Decimal)_Height / SoftConfigs.Delegates) + (_Height % SoftConfigs.Delegates > 0 ? 1 : 0);
        }

        public static Byte[] GetBytes()
        {
            try
            {
                var assetBytes = private.types[trs.type].getBytes.call(this, trs, skipSignature, skipSecondSignature);
        var assetSize = assetBytes ? assetBytes.length : 0;

        var bb = new ByteBuffer(1 + 4 + 32 + 32 + 8 + 8 + 64 + 64 + assetSize, true);
        bb.writeByte(trs.type);
    bb.writeInt(trs.timestamp);

    var senderPublicKeyBuffer = new Buffer(trs.senderPublicKey, 'hex');
    for (var i = 0; i<senderPublicKeyBuffer.length; i++) {
      bb.writeByte(senderPublicKeyBuffer[i]);
    }

    if (trs.requesterPublicKey) {
      var requesterPublicKey = new Buffer(trs.requesterPublicKey, 'hex');
      for (var i = 0; i<requesterPublicKey.length; i++) {
        bb.writeByte(requesterPublicKey[i]);
      }
    }

    if (trs.recipientId) {
      if (/^[0-9]{1,20}$/g.test(trs.recipientId)) {
        var recipient = bignum(trs.recipientId).toBuffer({ size: 8 });
        for (var i = 0; i< 8; i++) {
          bb.writeByte(recipient[i] || 0);
        }
      } else {
        bb.writeString(trs.recipientId);
      }
    } else {
      for (var i = 0; i< 8; i++) {
        bb.writeByte(0);
      }
    }

    bb.writeLong(trs.amount);

    if (trs.message) bb.writeString(trs.message);
    if (trs.args) {
      for (var i = 0; i<trs.args.length; ++i) {
        bb.writeString(trs.args[i])
      }
    }

    if (assetSize > 0) {
      for (var i = 0; i<assetSize; i++) {
        bb.writeByte(assetBytes[i]);
      }
    }

    if (!skipSignature && trs.signature) {
      var signatureBuffer = new Buffer(trs.signature, 'hex');
      for (var i = 0; i<signatureBuffer.length; i++) {
        bb.writeByte(signatureBuffer[i]);
      }
    }

    if (!skipSecondSignature && trs.signSignature) {
      var signSignatureBuffer = new Buffer(trs.signSignature, 'hex');
      for (var i = 0; i<signSignatureBuffer.length; i++) {
        bb.writeByte(signSignatureBuffer[i]);
      }
    }

    bb.flip();
  } catch (e) {
    throw Error(e.toString());
  }
  return bb.toBuffer();
        }
    }
}
