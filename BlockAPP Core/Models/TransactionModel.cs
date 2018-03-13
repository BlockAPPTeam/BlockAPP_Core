using System;

namespace BlockAPP_Core.Models
{
    public class Transaction
    {
        public String Id { get; set; }
        public Enums.TransactionType Type { get; set; }

        public Decimal Amount { get; set; }
        public Decimal Fee { get; set; }

        public UInt64 RecipientId { get; set; }
        public UInt64 SenderId { get; set; }
        public String SenderPublicKey { get; set; }
        public String Signature { get; set; }

        public int Timestamp { get; set; }
    }
}
