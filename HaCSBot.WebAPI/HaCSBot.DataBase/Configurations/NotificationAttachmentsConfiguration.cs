using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class NotificationAttachmentsConfiguration : IEntityTypeConfiguration<NotificationAttachment>
    {
        public void Configure(EntityTypeBuilder<NotificationAttachment> builder)
        {
            builder.ToTable("NotificationAttachments");

            builder.HasKey(na => na.Id);

            builder.Property(na => na.Type)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(na => na.TelegramFileId)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(na => na.Caption)
                   .HasMaxLength(1024);

            builder.HasOne(na => na.Notification)
                   .WithMany(n => n.Attachments)
                   .HasForeignKey(na => na.NotificationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
