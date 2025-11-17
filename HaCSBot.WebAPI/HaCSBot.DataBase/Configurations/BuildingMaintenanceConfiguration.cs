using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class BuildingMaintenanceConfiguration : IEntityTypeConfiguration<BuildingMaintenance>
    {
        public void Configure(EntityTypeBuilder<BuildingMaintenance> builder)
        {
            builder.ToTable("BuildingMaintenances");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Type).IsRequired(); 
            builder.Property(m => m.Description).IsRequired().HasMaxLength(1000);
            builder.Property(m => m.Status).IsRequired(); 
            builder.Property(m => m.CreatedDate).IsRequired();
            builder.Property(m => m.PlannedEndDate).IsRequired(false); 

            // Relationships
            builder.HasOne(m => m.Building)
                   .WithMany() 
                   .HasForeignKey(m => m.BuildingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.User)
                   .WithMany() 
                   .HasForeignKey(m => m.UserId)
                   .OnDelete(DeleteBehavior.SetNull); 

            
            builder.HasOne<User>() 
                   .WithMany()
                   .HasForeignKey(m => m.AdminId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
