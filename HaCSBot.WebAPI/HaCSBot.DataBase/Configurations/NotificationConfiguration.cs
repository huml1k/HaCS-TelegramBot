using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.ID); 
            builder.Property(n => n.SentDate).IsRequired();
            builder.Property(n => n.Type).IsRequired(); 
            builder.Property(n => n.Message).IsRequired().HasMaxLength(2000);

            
            builder.HasOne(n => n.BuildingMaintenance)
                   .WithMany() 
                   .HasForeignKey(n => n.BuildingMaintenanceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(n => n.House) 
                   .WithMany() 
                   .HasForeignKey(n => n.BuildingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
