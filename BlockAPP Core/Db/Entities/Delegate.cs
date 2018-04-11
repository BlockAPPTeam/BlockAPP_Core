using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BlockAPP_Core.Db.Entities
{
    [Table("Delegates")]
    public class DelegateAccount
    {
        [Key]
        public String PublicKey { get; set; }
        public String IP { get; set; }
        public String Port { get; set; }
    }
}
