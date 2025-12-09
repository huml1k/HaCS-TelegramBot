using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.DataBase.Repositories.Extensions
{
	public interface IComplaintRepository
	{
		Task AddAsync(Complaint complaint);
		Task UpdateAsync(Complaint complaint);
		Task<Complaint?> GetByIdAsync(Guid id);
		Task<Complaint?> GetByIdWithDetailsAsync(Guid id);
		Task<List<Complaint>> GetByApartmentIdAsync(Guid apartmentId);
		Task<List<Complaint>> GetByBuildingIdAsync(Guid buildingId, int page = 1, int pageSize = 20);
		Task<List<Complaint>> GetActiveForBuildingAsync(Guid buildingId);
		Task<List<Complaint>> GetByUserTelegramIdAsync(long telegramId);
		Task ChangeStatusAsync(Guid complaintId, ComplaintStatus newStatus);
		Task<List<Complaint>> GetUnprocessedAsync();
		Task DeleteAsync(Guid id);
		Task<bool> ExistsAsync(Guid id);
	}
}
