using HaCSBot.DataBase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.DataBase.Models
{
	public class NotificationAttachment
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid NotificationId { get; set; }
		public Notification Notification { get; set; } = null!;

		public AttachmentType Type { get; set; }           
		public required string TelegramFileId { get; set; } 
		public string? Caption { get; set; }               
	}
}
