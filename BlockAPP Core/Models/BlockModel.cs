using System;

namespace BlockAPP_Core.Models
{
    public class Block
    {
        public String Id { get; set; }
        public int Version { get; set; }

        public Decimal TotalAmount { get; set; }
        public Decimal TotalFee { get; set; }
        public Decimal Reward { get; set; }
        public Transaction[] Transactions { get; set; }
        public Asset[] Assets { get; set; }

        public String PreviousBlockId { get; set; }

        public int Timestamp { get; set; }
        public UInt64 Height { get; set; }

        public String GeneratorPublicKey { get; set; }
        public String BlockSignature { get; set; }
    }
}
