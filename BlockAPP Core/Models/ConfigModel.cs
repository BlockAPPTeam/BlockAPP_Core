using System;
using System.Collections.Generic;
using System.Text;

namespace BlockAPP_Core.Models
{
    public class Config
    {
        public int Port { get; set; }
        public String Address { get; set; }
        public String LocalIP { get; set; }
        public Enums.LogLevel LogLevel { get; set; }
        public String SecretKey { get; set; }

        public String[] ApiWhiteList { get; set; }
        public int PeersTimeout { get; set; }
        public String[] Peers { get; set; }
        public String[] PeersBlacklist { get; set; }

        public String DAppPassword { get; set; }
    }
}
