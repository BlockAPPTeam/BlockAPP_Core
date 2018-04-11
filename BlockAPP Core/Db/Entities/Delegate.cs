using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BlockAPP_Core.Db.Entities
{
    [Table("Delegates")]
    public class DelegateAccount
    {
        public String Id { get; set; }
        public String PublicKey { get; set; }
        public String IP { get; set; }
        public String Port { get; set; }
    }
}
