using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BlockAPP_Core.Db.Entities
{
    [Table("Accounts")]
    public class Account
    {
        public String PublicKey { get; set; }
    }
}
