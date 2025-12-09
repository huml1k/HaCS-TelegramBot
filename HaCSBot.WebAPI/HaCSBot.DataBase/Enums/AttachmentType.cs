using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.DataBase.Enums
{
	public enum AttachmentType
	{
		Photo = 0,
		Document = 1,     // PDF, Word, Excel
		Video = 2,
		Voice = 3,        // Голосовое сообщение от диспетчера
		VideoNote = 4,    // Кругляшок
		Animation = 5,    // GIF
		Sticker = 6       
	}
}
