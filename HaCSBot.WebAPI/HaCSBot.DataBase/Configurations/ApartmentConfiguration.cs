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

			// Правильная связь с Building
			builder.HasOne(a => a.Building)
				   .WithMany(b => b.Apartments)  // Если в Building есть свойство ICollection<Apartment> Apartments
				   .HasForeignKey(a => a.BuildingId)  // Правильно: внешний ключ BuildingId в таблице Apartments
				   .OnDelete(DeleteBehavior.Cascade);

			// Правильная связь с User
			builder.HasOne(a => a.User)
				   .WithMany(u => u.Apartments)
				   .HasForeignKey(a => a.UserId)
				   .OnDelete(DeleteBehavior.SetNull);
		}
	}
}
