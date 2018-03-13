using System;

namespace BlockAPP_Core.Models
{
    public class Asset
    {
        public String Id { get; set; }
        public Enums.AssetType Type { get; set; }
        public String Data { get; set; }

        public String PublicKey { get; set; }
        public String Signature { get; set; }
    }
}
