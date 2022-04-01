using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUrl
{
    public class MyDataContext : DbContext
    {
        public MyDataContext()
        {

        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string database = Path.Combine(@"D:\LSS\其他\myhome\replyApp\TestUrl\DB", "MyDatabase.db");
            optionsBuilder.UseSqlite($"Data Source={database};");
        }
        public DbSet<Logs> Logs { get; set; }
    }
}
