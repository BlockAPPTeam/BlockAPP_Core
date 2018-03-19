using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Core.Network.Enums
{
    public enum PacketType
    {
        TYPE_MyCredentials = 0,
        TYPE_Registered = 1,
        TYPE_HostExiting = 2,
        TYPE_ClientDisconnecting = 3,
        TYPE_Close = 4,
        TYPE_Message = 5,
        TYPE_MessageReceived = 6
    }
}
