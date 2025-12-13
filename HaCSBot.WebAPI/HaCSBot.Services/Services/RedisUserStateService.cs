//using HaCSBot.Services.Enums;
//using HaCSBot.Services.Services.Extensions;
//using Microsoft.Extensions.Caching.Distributed;
//using System.Text.Json;

//namespace HaCSBot.Services.Services
//{
//    public class RedisUserStateService : IUserStateService
//    {
//        private readonly IDistributedCache _cache;
//        private readonly TimeSpan _expiry = TimeSpan.FromHours(24); // настраивайте под себя

//        public RedisUserStateService(IDistributedCache cache)
//        {
//            _cache = cache;
//        }

//        private string StateKey(long telegramId) => $"bot:state:{telegramId}";
//        private string DataKey(long telegramId) => $"bot:data:{telegramId}";

//        public ConversationState GetState(long telegramId)
//        {
//            var value = _cache.GetString(StateKey(telegramId));
//            return value == null ? ConversationState.None : Enum.Parse<ConversationState>(value);
//        }

//        public void SetState(long telegramId, ConversationState state)
//        {
//            _cache.SetString(StateKey(telegramId), state.ToString(), new DistributedCacheEntryOptions
//            {
//                AbsoluteExpirationRelativeToNow = _expiry
//            });
//        }

//        public TempDialogData? GetTempData(long telegramId)
//        {
//            var json = _cache.GetString(DataKey(telegramId));
//            return json == null ? null : JsonSerializer.Deserialize<TempDialogData>(json);
//        }

//        public void SetTempData(long telegramId, TempDialogData data)
//        {
//            var json = JsonSerializer.Serialize(data);
//            _cache.SetString(DataKey(telegramId), json, new DistributedCacheEntryOptions
//            {
//                AbsoluteExpirationRelativeToNow = _expiry
//            });
//        }

//        public void ClearState(long telegramId)
//        {
//            _cache.Remove(StateKey(telegramId));
//        }

//        public void ClearTempData(long telegramId)
//        {
//            _cache.Remove(DataKey(telegramId));
//        }

//        public void ClearAll(long telegramId)
//        {
//            ClearState(telegramId);
//            ClearTempData(telegramId);
//        }
//    }
//}
