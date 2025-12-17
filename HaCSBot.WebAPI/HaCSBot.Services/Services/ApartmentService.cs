using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using Telegram.Bot.Types;

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

		public async Task<ApartmentDto> GetByIdAsync(Guid id)
		{
			//Получаем данные из репозитория
			var apartment = await _apartmentRepository.GetByIdAsync(id);

			// Маппим в DTO
			var apartmentDto = _mapper.Map<ApartmentDto>(apartment);

			//Возвращаем DTO
			return apartmentDto;
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

        public async Task<List<ApartmentDto>> GetApartmentsInBuildingAsync(Guid buildingId)
        {
            var apartments = await _apartmentRepository.GetApartmentsByBuildingIdAsync(buildingId);
            return _mapper.Map<List<ApartmentDto>>(apartments);
        }

        public async Task<ApartmentDto?> GetByIdAsync(Guid apartmentId)
        {
            var apartment = await _apartmentRepository.GetByIdAsync(apartmentId);
            return _mapper.Map<ApartmentDto>(apartment);
        }
    }
}
