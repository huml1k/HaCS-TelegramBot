using AutoMapper;
using HaCSBot.Contracts.Contracts;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Contracts
{
    public class AutoMapping : Profile
    {
        public AutoMapping() 
        {
            this.CreateMap<UserRegistrationContract, User>();
        }
    }
}
