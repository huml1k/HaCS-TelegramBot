using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
    {
        public void Configure(EntityTypeBuilder<Complaint> builder)
        {
            builder.ToTable("Complaints");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Category)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(c => c.Description)
                   .IsRequired()
                   .HasMaxLength(3000);

            builder.Property(c => c.Status)
                   .IsRequired()
                   .HasDefaultValue(ComplaintStatus.New)
                   .HasConversion<string>();

            builder.Property(c => c.CreatedDate)
                   .IsRequired();

            builder.HasOne(c => c.Apartment)
                   .WithMany()
                   .HasForeignKey(c => c.ApartmentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Attachments)
                   .WithOne(ca => ca.Complaint)
                   .HasForeignKey(ca => ca.ComplaintId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
