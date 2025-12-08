using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.Services.Enums
{
	public enum ConversationState
	{
		None,
		AwaitingFirstName,
		AwaitingLastName,
		AwaitingPhone
	}
}
