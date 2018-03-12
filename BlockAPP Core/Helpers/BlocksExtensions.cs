using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class BlocksExtensions
    {
        public static UInt64 GetId(this Models.Block _Block)
        {
            StringBuilder _SB = new StringBuilder();
            _SB.Append(_Block.Version);
            _SB.Append(_Block.Timestamp);
            _SB.Append(_Block.PreviousBlockId);
            _SB.Append(_Block.NumberOfTransactions);
            _SB.Append(_Block.TotalAmount);
            _SB.Append(_Block.TotalFee);
            _SB.Append(_Block.Reward);
            _SB.Append(_Block.PayloadLength);
            _SB.Append(_Block.PayloadHash);
            _SB.Append(_Block.GeneratorPublicKey);
            _SB.Append(_Block.BlockSignature);
            
            using (var _SHA256 = SHA256.Create())
            {
                var _Bytes = _SHA256.ComputeHash(Encoding.UTF8.GetBytes(_SB.ToString()));
                return BitConverter.ToUInt64(_Bytes, 0);
            }
        }

        public static Decimal CalculateFee(this Models.Block _Block)
        {
            return 1;
        }

        public static Models.Block CreateBlock(this Models.Block _Block)
        {
            //_Block.Height = _Block.PreviousBlockId

            return _Block;
        }

        public static Models.Block VerifySignature(this Models.Block _Block)
        {
            //_Block.Height = _Block.PreviousBlockId

            return _Block;
        }
    }
}
