using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.Contracts.DTOs
{
	public class UserLoginDto
	{
		public long TelegramId { get; set; }
		public string Phone { get; set; } = string.Empty; 
	}
}
