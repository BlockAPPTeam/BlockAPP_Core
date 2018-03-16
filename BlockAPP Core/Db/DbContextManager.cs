using System;
using System.Collections.Generic;
using System.Text;
using BlockAPP_Core.Db.Concrete;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BlockAPP_Core.Db
{
    public static class DbContextManager
    {
        public static EFDbContext _Db;

        public static void InitConnection()
        {
            if (_Db == null)
            {
                var _OptionsBuilder = new DbContextOptionsBuilder<EFDbContext>();
                _OptionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=BlockAPP;User Id=postgres;Password=5213612;");
                _Db = new EFDbContext(_OptionsBuilder.Options);
                _Db.Database.EnsureCreated();
            }
        }
       
    }
}
