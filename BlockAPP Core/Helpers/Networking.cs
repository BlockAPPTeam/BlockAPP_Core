using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace BlockAPP_Core.Helpers
{
    public static class Networking
    {
        public static String GetLocalIP()
        {
            String _IP = null;
            foreach (NetworkInterface _NI in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (_NI.OperationalStatus == OperationalStatus.Up) //&& item.NetworkInterfaceType == ?
                {
                    foreach (UnicastIPAddressInformation _II in _NI.GetIPProperties().UnicastAddresses)
                    {
                        if (_II.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(_II.Address))
                        {
                            _IP = _II.Address.ToString();
                        }
                    }
                }
            }

            return _IP;
        }
    }
}
