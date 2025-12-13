using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
	public interface IComplaintService
	{
        public Task<Guid> CreateComplaintAsync(CreateComplaintDto dto, long telegramId);
        public Task<List<ComplaintDto>> GetMyComplaintsAsync(long telegramId);
        public Task<List<ComplaintDto>> GetNewComplaintsForAdminAsync(Guid adminId);
        public Task<List<ComplaintDto>> GetComplaintsByBuildingAsync(Guid buildingId);
        public Task<ComplaintStatus> ChangeComplaintStatusAsync(Guid complaintId, ComplaintStatus status, Guid adminId);
        public Task<ComplaintDetailsDto> GetComplaintDetailsAsync(Guid complaintId);
    }
}
