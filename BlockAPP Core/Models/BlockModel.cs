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
        public String PayloadHash { get; set; }
        public int Timestamp { get; set; }
        public int NumberOfTransactions { get; set; }
        public int PayloadLength { get; set; }
        public UInt64 PreviousBlockId { get; set; }
        public String GeneratorPublicKey { get; set; }
        public Transaction[] Transactions { get; set; }
        public int Height { get; set; }
        public String BlockSignature { get; set; }
    }
}
