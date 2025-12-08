using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.Services.Services
{
	public class InMemoryUserStateService : IUserStateService
	{
		// Хранилища в памяти (в продакшене замени на Redis или БД)
		private readonly ConcurrentDictionary<long, ConversationState> _states = new();
		private readonly ConcurrentDictionary<long, RegistrationData> _tempData = new();

		public ConversationState GetState(long telegramId)
		{
			return _states.GetValueOrDefault(telegramId, ConversationState.None);
		}

		public void SetState(long telegramId, ConversationState state)
		{
			_states[telegramId] = state;
		}

		public void ClearState(long telegramId)
		{
			_states.TryRemove(telegramId, out _);
		}

		public RegistrationData? GetTempRegistrationData(long telegramId)
		{
			return _tempData.TryGetValue(telegramId, out var data) ? data : null;
		}

		public void SetTempRegistrationData(long telegramId, RegistrationData data)
		{
			_tempData[telegramId] = data;
		}

		public void ClearTempRegistrationData(long telegramId)
		{
			_tempData.TryRemove(telegramId, out _);
		}
	}
}
