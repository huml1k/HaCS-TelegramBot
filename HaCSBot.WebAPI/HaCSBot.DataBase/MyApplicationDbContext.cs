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
        public DbSet<House> Houses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BuildingMaintenance> BuildingMaintenances { get; set; }
    }
}
