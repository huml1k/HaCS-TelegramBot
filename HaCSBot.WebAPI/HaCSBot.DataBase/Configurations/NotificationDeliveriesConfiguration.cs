using HaCSBot.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HaCSBot.DataBase.Configurations
{
    public class NotificationDeliveriesConfiguration : IEntityTypeConfiguration<NotificationDelivery>
    {
        public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
        {
            builder.ToTable("NotificationDeliveries");

            builder.HasKey(nd => nd.Id);

            builder.Property(nd => nd.TelegramUserId)
                   .IsRequired();

            builder.HasOne(nd => nd.Notification)
                   .WithMany(n => n.Deliveries)
                   .HasForeignKey(nd => nd.NotificationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
