using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class ComplaintAttachmentConfiguration : IEntityTypeConfiguration<ComplaintAttachment>
    {
        public void Configure(EntityTypeBuilder<ComplaintAttachment> builder)
        {
            builder.ToTable("ComplaintAttachments");

            builder.HasKey(ca => ca.Id);

            builder.Property(ca => ca.Type)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(ca => ca.TelegramFileId)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(ca => ca.Caption)
                   .HasMaxLength(1024);

            builder.HasOne(ca => ca.Complaint)
                   .WithMany(c => c.Attachments)
                   .HasForeignKey(ca => ca.ComplaintId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
