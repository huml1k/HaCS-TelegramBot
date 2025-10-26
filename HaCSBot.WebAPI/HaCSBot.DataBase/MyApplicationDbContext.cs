using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase
{
    public class MyApplicationDbContext : DbContext
    {
        public MyApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
