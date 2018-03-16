using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlockAPP_Core.Db.Entities
{
    [Table("Blocks")]
    public class Block
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(256)]
        public String BlockId { get; set; }
        [MaxLength(1024)]
        public String Path { get; set; }
    }
}
