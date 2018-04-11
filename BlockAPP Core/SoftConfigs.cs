using System;

namespace BlockAPP_Core
{
    public static class SoftConfigs
    {
        public static DateTime StartDelegates = new DateTime(2018, 5, 25);
        public const int BlockInterval = 10;
        public const int Delegates = 101;

        public static Models.Config _LocalConfig { get; set; }
    }
}
