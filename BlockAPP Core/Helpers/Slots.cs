﻿using System;
using System.Numerics;

namespace BlockAPP_Core.Helpers
{
    public static class Slots
    {
        public static UInt64 GetCurrentSlotNumber()
        {
            var _Spended = SoftConfigs.StartDelegates - DateTime.Now;
            return (UInt64)Math.Floor(_Spended.TotalSeconds / SoftConfigs.BlockInterval);
        }

        public static int GetSlotPadding(Models.Block _Block)
        {
            BigInteger _Id = BigInteger.Parse(_Block.Id);
            while (_Id > SoftConfigs.Delegates)
            {
                _Id /= 2;
            }

            return Convert.ToInt32(_Id);
        }
    }
}
