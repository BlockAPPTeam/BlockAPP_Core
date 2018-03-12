using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class Slots
    {
        public static long BeginEpochTime()
        {
            Int32 _UnixTimestamp = (Int32)(SoftConfigs._StartDelegates.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return _UnixTimestamp;
        }

        public static long GetEpochTime(long _Timestamp)
        {
            Int32 _UnixTimestamp = (Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return (long)Math.Floor(((double)_Timestamp - (double)_UnixTimestamp) / 1000);
        }
    }
}
