using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.Services.Services
{
    public class TariffService : ITariffService
    {
        private readonly ITariffRepository _repository;

        public TariffService(ITariffRepository repository)
        {
            _repository = repository;
        }
    }
}