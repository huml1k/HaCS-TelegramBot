using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Services.Services.Extensions
{
	public interface IComplaintService
	{
		Task<ComplaintDto> CreateComplaintAsync(CreateComplaintDto dto, long telegramId);
		Task<List<ComplaintDto>> GetMyComplaintsAsync(long telegramId);
		Task<ComplaintDetailsDto> GetComplaintDetailsAsync(Guid complaintId, long telegramId);
		Task<List<ComplaintDto>> GetNewComplaintsForAdminAsync(Guid adminId);
		Task<List<ComplaintDto>> GetComplaintsByBuildingAsync(Guid buildingId);
		Task<ComplaintDto> ChangeComplaintStatusAsync(ComplaintStatusChangeDto dto, Guid adminId);
		Task<bool> CanUserAccessComplaintAsync(Guid complaintId, long telegramId);
	}
}
