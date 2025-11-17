using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class BuildingConfiguration : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            builder.ToTable("Buildings");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.StreetType).IsRequired(); 
            builder.Property(b => b.StreetName).IsRequired().HasMaxLength(100);
            builder.Property(b => b.BuildingNumber).IsRequired().HasMaxLength(20);
        }
    }
}
