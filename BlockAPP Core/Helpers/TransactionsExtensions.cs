using System;

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

        }
    }
}
