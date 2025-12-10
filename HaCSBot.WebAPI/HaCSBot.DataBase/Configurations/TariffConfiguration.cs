using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class TariffConfiguration : IEntityTypeConfiguration<Tariff>
    {
        public void Configure(EntityTypeBuilder<Tariff> builder)
        {
            builder.ToTable("Tariffs");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(t => t.Price)
                   .IsRequired();

            builder.Property(t => t.ValidFrom)
                   .IsRequired();

            // Может быть null — тариф действует "вечно"
            builder.Property(t => t.ValidTo);
        }
    }
}