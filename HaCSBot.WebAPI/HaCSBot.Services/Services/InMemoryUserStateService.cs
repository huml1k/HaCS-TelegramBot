using HaCSBot.Contracts.DTOs;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using System.Collections.Concurrent;

namespace HaCSBot.Services.Services
{
	public class InMemoryUserStateService : IUserStateService
	{
		// Хранилища в памяти (в продакшене замени на Redis или БД)
		private readonly ConcurrentDictionary<long, ConversationState> _states = new();
		private readonly ConcurrentDictionary<long, RegistrationTempDto> _tempData = new();
		private readonly ConcurrentDictionary<long, ComplaintTempDto> _tempDataComaplaint = new();
		private readonly ConcurrentDictionary<long, MeterReadingTempDto> _tempDataMeter = new();

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

		public RegistrationTempDto? GetTempRegistrationData(long telegramId)
		{
			return _tempData.TryGetValue(telegramId, out var data) ? data : null;
		}

		public void SetTempRegistrationData(long telegramId, RegistrationTempDto data)
		{
			_tempData[telegramId] = data;
		}

		public void ClearTempRegistrationData(long telegramId)
		{
			_tempData.TryRemove(telegramId, out _);
		}

		public ComplaintTempDto? GetTempComplaintData(long telegramId)
		{
			return _tempDataComaplaint.TryGetValue(telegramId, out var data) ? data : null;
		}

		public void SetTempComplaintData(long telegramId, ComplaintTempDto data)
		{
			_tempDataComaplaint[telegramId] = data;
		}

		public void ClearTempComplaintData(long telegramId)
		{
			_tempDataComaplaint.TryRemove(telegramId, out _);
		}

		public MeterReadingTempDto? GetTempMeterData(long telegramId)
		{
			return _tempDataMeter.TryGetValue(telegramId, out var data) ? data : null;
		}

		public void SetTempMeterData(long telegramId, MeterReadingTempDto data)
		{
			_tempDataMeter[telegramId] = data;
		}

		public void ClearTempMeterData(long telegramId)
		{
			_tempDataMeter.TryRemove(telegramId, out _);
		}
	}
}
