using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Core.Network.Enums
{
    public enum PacketType
    {
        TYPE_MyCredentials = 0,
        TYPE_RequestCredentials = 1,
        TYPE_Registered = 2,
        TYPE_HostExiting = 3,
        TYPE_ClientDisconnecting = 4,
        TYPE_Close = 5,
        TYPE_Message = 6,
        TYPE_MessageReceived = 7
    }
}
