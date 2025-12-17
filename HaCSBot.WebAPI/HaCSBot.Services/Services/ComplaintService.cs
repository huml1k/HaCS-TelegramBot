using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
	public class ComplaintService : IComplaintService
	{
		private readonly IComplaintRepository _complaintRepository;
		private readonly IApartmentRepository _apartmentRepository;
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;

		public ComplaintService(
			IComplaintRepository complaintRepository,
			IApartmentRepository apartmentRepository,
			IUserRepository userRepository,
			IMapper mapper)
		{
			_complaintRepository = complaintRepository;
			_apartmentRepository = apartmentRepository;
			_userRepository = userRepository;
			_mapper = mapper;
		}

		public async Task<ComplaintDto> CreateComplaintAsync(CreateComplaintDto dto, long telegramId)
		{
			// Получаем пользователя
			var user = await _userRepository.GetByTelegramIdAsync(telegramId);
			if (user == null)
				throw new InvalidOperationException("Пользователь не найден");

			// Проверяем квартиру
			var apartment = await _apartmentRepository.GetByIdAsync(dto.ApartmentId);
			if (apartment == null || apartment.UserId != user.Id)
				throw new InvalidOperationException("Квартира не найдена или не принадлежит вам");

			// Маппим DTO в модель
			var complaint = _mapper.Map<Complaint>(dto);
			complaint.CreatedDate = DateTime.UtcNow;
			complaint.Status = ComplaintStatus.New;

			// Сохраняем
			await _complaintRepository.AddAsync(complaint);

			// Возвращаем DTO
			return _mapper.Map<ComplaintDto>(complaint);
		}

		public async Task<List<ComplaintDto>> GetMyComplaintsAsync(long telegramId)
		{
			var complaints = await _complaintRepository.GetByUserTelegramIdAsync(telegramId);
			return _mapper.Map<List<ComplaintDto>>(complaints);
		}

		public async Task<ComplaintDetailsDto> GetComplaintDetailsAsync(Guid complaintId, long telegramId)
		{
			var complaint = await _complaintRepository.GetByIdAsync(complaintId);
			if (complaint == null)
				throw new InvalidOperationException("Жалоба не найдена");

			// Проверяем права доступа
			var user = await _userRepository.GetByTelegramIdAsync(telegramId);
			if (user == null)
				throw new InvalidOperationException("Пользователь не найден");

			if (user.Role != Roles.Admin)
				throw new UnauthorizedAccessException("Нет доступа к этой жалобе");

			return _mapper.Map<ComplaintDetailsDto>(complaint);
		}

		public async Task<List<ComplaintDto>> GetNewComplaintsForAdminAsync(Guid adminId)
		{
			var user = await _userRepository.GetByIdAsync(adminId);
			if (user?.Role != Roles.Admin)
				throw new UnauthorizedAccessException("Требуются права администратора");

			var complaints = await _complaintRepository.GetUnprocessedAsync();
			return _mapper.Map<List<ComplaintDto>>(complaints);
		}

        public async Task<List<ComplaintDto>> GetAllComplaintsForAdminAsync(Guid adminId)
        {
            var user = await _userRepository.GetByIdAsync(adminId);
            if (user?.Role != Roles.Admin)
                throw new UnauthorizedAccessException("Требуются права администратора");

            var complaints = await _complaintRepository.GetAllAsync();
            return _mapper.Map<List<ComplaintDto>>(complaints);
        }

        public async Task<List<ComplaintDto>> GetComplaintsByBuildingAsync(Guid buildingId)
		{
			var complaints = await _complaintRepository.GetByBuildingIdAsync(buildingId, 1, 100);
			return _mapper.Map<List<ComplaintDto>>(complaints);
		}

		public async Task<ComplaintDto> ChangeComplaintStatusAsync(ComplaintStatusChangeDto dto, Guid adminId)
		{
			// Проверяем права администратора
			var admin = await _userRepository.GetByIdAsync(adminId);
			if (admin?.Role != Roles.Admin)
				throw new UnauthorizedAccessException("Требуются права администратора");

			// Получаем жалобу
			var complaint = await _complaintRepository.GetByIdAsync(dto.ComplaintId);
			if (complaint == null)
				throw new InvalidOperationException("Жалоба не найдена");

			// Обновляем статус
			complaint.Status = dto.Status;

			if (dto.Status == ComplaintStatus.Resolved || dto.Status == ComplaintStatus.Closed)
				complaint.ResolvedDate = DateTime.UtcNow;

			await _complaintRepository.UpdateAsync(complaint);

			return _mapper.Map<ComplaintDto>(complaint);
		}

		public async Task<bool> CanUserAccessComplaintAsync(Guid complaintId, long telegramId)
		{
			var complaint = await _complaintRepository.GetByIdAsync(complaintId);
			if (complaint == null) return false;

			var user = await _userRepository.GetByTelegramIdAsync(telegramId);
			if (user == null) return false;

			// Пользователь имеет доступ если:
			// 1. Он владелец квартиры
			// 2. Он администратор
			return complaint.Apartment.UserId == user.Id || user.Role == Roles.Admin;
		}
	}
}
