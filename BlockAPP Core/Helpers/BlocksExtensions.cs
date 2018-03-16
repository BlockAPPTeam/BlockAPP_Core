using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class BlocksExtensions
    {
        public static String GetId(this Models.Block _Block)
        {
            var _BlockJSON = JsonConvert.SerializeObject(_Block);

            return Hashing.GetId(_BlockJSON).ToString();
        }

        public static Decimal CalculateFee(this Models.Block _Block)
        {
            return 1;
        }

        public static Models.Block FinalizeBlock(this Models.Block _Block)
        {
            //_Block.Height = _Block.PreviousBlockId

            return _Block;
        }

        public static Boolean Verify(this Models.Block _Block)
        {
            var _BlockJSON = JsonConvert.SerializeObject(_Block);

            var _BlockHash = Hashing.GetHashForString(_BlockJSON);
            return RSA.Verify(_BlockHash, _Block.BlockSignature, _Block.GeneratorPublicKey);
        }
    }
}
