using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public class BlockLoader
    {
        public static Models.Block LoadBlock(String _Path)
        {
            var _Data = File.ReadAllBytes(_Path);
            String _Json = DataCompressor.Unzip(_Data);

            var _Block = JsonConvert.DeserializeObject<Models.Block>(_Json);
            return _Block;
        }

        public static void StoreBlock()
        {
            // ToDo
        }
    }
}
