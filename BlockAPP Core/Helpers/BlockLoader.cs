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


        }
    }
}
