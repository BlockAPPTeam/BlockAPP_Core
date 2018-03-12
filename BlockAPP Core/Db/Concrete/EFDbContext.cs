using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using BlockAPP_Core.Models;
using System.Threading.Tasks;

namespace BlockAPP_Core.Db.Concrete
{
    public class EFDbContext : DbContext
    {
        public EFDbContext(DbContextOptions<EFDbContext> options) : base(options)
        {

        }
        public DbSet<Block> Blocks { get; set; }

        //
        #region Users

        public async Task AddBlock(Block _Model)
        {
            this.Blocks.Add(_Model);
            await this.SaveChangesAsync();
        }
        public async Task EditBlock(Block _Model)
        {
            this.Update(_Model);
            await this.SaveChangesAsync();
        }
        public async Task DeleteBlock(int _Id)
        {
            await this.Database.ExecuteSqlCommandAsync("DELETE FROM \"Blocks\" WHERE \"Id\" = '" + _Id + "';");
        }

        #endregion
        //

        public void AddNoSave(Object _Model)
        {
            this.Add(_Model);
        }
    }
}
