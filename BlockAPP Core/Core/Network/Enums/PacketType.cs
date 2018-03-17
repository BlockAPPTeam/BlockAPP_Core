using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Core.Network.Enums
{
    public enum PacketType
    {
        TYPE_RequestCredentials = 0,
        TYPE_MyCredentials = 1,
        TYPE_Registered = 2,
        TYPE_HostExiting = 3,
        TYPE_ClientData = 4,
        TYPE_ClientDisconnecting = 5,
        TYPE_CredentialsUpdate = 6,
        TYPE_Close = 7,
        TYPE_Message = 8,
        TYPE_MessageReceived = 9
    }
}
