using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services
{
	public class ComplaintService : IComplaintService
	{
        private readonly IComplaintRepository _complaintRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IUserRepository _userRepository;

        public ComplaintService(IComplaintRepository complaintRepository,
            IApartmentRepository apartmentRepository,
            IUserRepository userRepository)
        {
            _complaintRepository = complaintRepository;
            _apartmentRepository = apartmentRepository;
            _userRepository = userRepository;
        }

        public async Task<Guid> CreateComplaintAsync(CreateComplaintDto dto, long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user == null) throw new InvalidOperationException("User not found");

            var apartment = await _apartmentRepository.GetByIdAsync(dto.ApartmentId);
            if (apartment == null || apartment.UserId != user.Id) throw new InvalidOperationException("Apartment not found or not owned");

            var complaint = new Complaint
            {
                ApartmentId = dto.ApartmentId,
                Category = dto.Category,
                Description = dto.Description,
                Status = ComplaintStatus.New,
                CreatedDate = DateTime.UtcNow,
                Attachments = dto.Attachments.Select(a => new ComplaintAttachment { Type = a.Type, TelegramFileId = a.TelegramFileId, Caption = a.Caption }).ToList()
            };
            await _complaintRepository.AddAsync(complaint);
            return complaint.Id;
        }

        public async Task<List<ComplaintDto>> GetMyComplaintsAsync(long telegramId)
        {
            var complaints = await _complaintRepository.GetByUserTelegramIdAsync(telegramId);
            return complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                Description = c.Description,
                Status = c.Status
            }).ToList();
        }

        public async Task<List<ComplaintDto>> GetNewComplaintsForAdminAsync(Guid adminId)
        {
            var complaints = await _complaintRepository.GetUnprocessedAsync();

            return complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                Description = c.Description
            }).ToList();
        }

        public async Task<List<ComplaintDto>> GetComplaintsByBuildingAsync(Guid buildingId)
        {
            var complaints = await _complaintRepository.GetByBuildingIdAsync(buildingId, 1, 100);
            return complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                Description = c.Description
            }).ToList();
        }

        public async Task<ComplaintStatus> ChangeComplaintStatusAsync(Guid complaintId, ComplaintStatus status, Guid adminId)
        {
            await _complaintRepository.ChangeStatusAsync(complaintId, status);
            return status;
        }

        public async Task<ComplaintDetailsDto> GetComplaintDetailsAsync(Guid complaintId)
        {
            var complaint = await _complaintRepository.GetByIdWithDetailsAsync(complaintId);
            if (complaint == null) throw new InvalidOperationException("Complaint not found");

            return new ComplaintDetailsDto
            {
                Id = complaint.Id,
                Description = complaint.Description,
                Attachments = complaint.Attachments.Select(a => a.TelegramFileId).ToList()
            };
        }
    }
}
