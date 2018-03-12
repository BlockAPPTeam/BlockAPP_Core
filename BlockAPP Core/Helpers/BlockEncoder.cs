using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class BlockEncoder
    {
        private static String GetHash(String _Data)
        {
            using (var _SHA256 = SHA256.Create())
            {
                var _Bytes = _SHA256.ComputeHash(Encoding.UTF8.GetBytes(_Data));
                return BitConverter.ToString(_Bytes);
            }
        }

        public static Boolean VerifyGenesisBlock(Models.Block _Block)
        {
            try
            {
                var payloadHash = crypto.createHash('sha256');
                var payloadLength = 0;

                for (var i = 0; i < block.transactions.length; ++i)
                {
                    var trs = block.transactions[i];
                    var bytes = scope.base.transaction.getBytes(trs);
                    payloadLength += bytes.length;
                    payloadHash.update(bytes);
                }
                var id = scope.base.block.getId(block);
                assert.equal(payloadLength, block.payloadLength, 'Unexpected payloadLength');
                assert.equal(payloadHash.digest().toString('hex'), block.payloadHash, 'Unexpected payloadHash');
                assert.equal(id, block.id, 'Unexpected block id');
                // assert.equal(id, '11839820784468442760', 'Block id is incorrect');
            }
            catch (e)
            {
                assert(false, 'Failed to verify genesis block: ' + e);
            }
        }


        public static void EncodeBlock(Models.Block _Block)
        {

        }
    }
}
