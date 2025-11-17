using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
    {
        public void Configure(EntityTypeBuilder<Apartment> builder)
        {
            builder.ToTable("Apartments");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.ApartmentNumber).IsRequired().HasMaxLength(10);

            
            builder.HasOne(a => a.Building)
                   .WithMany() 
                   .HasForeignKey(a => a.BuildingId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(a => a.User)
                   .WithMany(u => u.Apartments)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
