using HaCSBot.DataBase.Configurations;
using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase
{
    public class MyApplicationDbContext : DbContext
    {
		public MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options)
			: base(options)
		{
			Database.EnsureCreated(); 
		}
		public DbSet<User> Users { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
        public DbSet<NotificationAttachment> NotificationAttachments { get; set; }
        public DbSet<BuildingMaintenance> BuildingMaintenances { get; set; }
        public DbSet<MeterReading> MeterReading { get; set; }
        public DbSet<ComplaintAttachment> ComplaintAttachments { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
		public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ApartmentConfiguration());
            modelBuilder.ApplyConfiguration(new BuildingConfiguration());
            modelBuilder.ApplyConfiguration(new BuildingMaintenanceConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
		}
    }
}
