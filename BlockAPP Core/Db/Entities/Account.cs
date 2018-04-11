using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BlockAPP_Core.Db.Entities
{
    [Table("Accounts")]
    public class Account
    {
        [Key]
        public String PublicKey { get; set; }
    }
}
