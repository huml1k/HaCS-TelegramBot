using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using System.Collections.Concurrent;

namespace HaCSBot.Services.Services
{
	public class InMemoryUserStateService : IUserStateService
	{
		// Хранилища в памяти (в продакшене замени на Redis или БД)
		private readonly ConcurrentDictionary<long, ConversationState> _states = new();
		private readonly ConcurrentDictionary<long, RegistrationData> _tempData = new();
        private readonly ConcurrentDictionary<long, ComplaintTempData> _tempDataComaplaint = new();
        private readonly ConcurrentDictionary<long, MeterTempData> _tempDataMeter = new();

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

        public ComplaintTempData? GetTempComplaintData(long telegramId)
        {
            return _tempDataComaplaint.TryGetValue(telegramId, out var data) ? data : null;
        }

        public void SetTempComplaintData(long telegramId, ComplaintTempData data)
        {
            _tempDataComaplaint[telegramId] = data;
        }

        public void ClearTempComplaintData(long telegramId)
        {
            _tempDataComaplaint.TryRemove(telegramId, out _);
        }

        public MeterTempData? GetTempMeterData(long telegramId)
        {
            return _tempDataMeter.TryGetValue(telegramId, out var data) ? data : null;
        }

        public void SetTempMeterData(long telegramId, MeterTempData data)
        {
            _tempDataMeter[telegramId] = data;
        }

        public void ClearTempMeterData(long telegramId)
        {
            _tempDataMeter.TryRemove(telegramId, out _);
        }
    }
}
