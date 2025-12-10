using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly IMeterReadingRepository _repository;

        public MeterReadingService(IMeterReadingRepository repository)
        {
            _repository = repository;
        }
    }
}
