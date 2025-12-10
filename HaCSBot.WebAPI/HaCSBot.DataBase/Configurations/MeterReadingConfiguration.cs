using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
    {
        public void Configure(EntityTypeBuilder<MeterReading> builder)
        {
            builder.ToTable("MeterReadings");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Type)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(m => m.Value)
                   .IsRequired();

            builder.Property(m => m.ReadingDate)
                   .IsRequired();

            builder.HasOne(m => m.Apartment)
                   .WithMany()
                   .HasForeignKey(m => m.ApartmentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
