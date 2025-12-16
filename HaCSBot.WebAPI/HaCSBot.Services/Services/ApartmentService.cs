using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class ApartmentService : IApartmentService
    {
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IMapper _mapper;

        public ApartmentService(
            IApartmentRepository apartmentRepository,
            IMapper mapper)
        {
            _apartmentRepository = apartmentRepository;
            _mapper = mapper;
        }

        public async Task<List<ApartmentDto>> GetByUserIdAsync(Guid userId)
        {
			//Получаем данные из репозитория
			var apartments = await _apartmentRepository.GetByUserIdAsync(userId);

			// Маппим в DTO
			var apartmentDtos = _mapper.Map<List<ApartmentDto>>(apartments);

			//Возвращаем DTO
			return apartmentDtos;
		}
    }
}
