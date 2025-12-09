using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.DataBase.Repositories
{
	public class ComplaintRepository : IComplaintRepository
	{
		private readonly MyApplicationDbContext _context;

		public ComplaintRepository(MyApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Complaint complaint)
		{
			await _context.Complaints.AddAsync(complaint);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(Complaint complaint)
		{
			_context.Complaints.Update(complaint);
			await _context.SaveChangesAsync();
		}

		public async Task<Complaint?> GetByIdAsync(Guid id)
		{
			return await _context.Complaints
				.FirstOrDefaultAsync(c => c.Id == id);
		}

		public async Task<Complaint?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.Complaints
				.AsNoTracking()
				.Include(c => c.Apartment)
					.ThenInclude(a => a!.User)
				.Include(c => c.Apartment)
					.ThenInclude(a => a!.Building)
				.Include(c => c.Attachments)
				.FirstOrDefaultAsync(c => c.Id == id);
		}

		public async Task<List<Complaint>> GetByApartmentIdAsync(Guid apartmentId)
		{
			return await _context.Complaints
				.AsNoTracking()
				.Where(c => c.ApartmentId == apartmentId)
				.OrderByDescending(c => c.CreatedDate)
				.Include(c => c.Attachments)
				.ToListAsync();
		}

		public async Task<List<Complaint>> GetByBuildingIdAsync(Guid buildingId, int page = 1, int pageSize = 20)
		{
			return await _context.Complaints
				.AsNoTracking()
				.Include(c => c.Apartment)
					.ThenInclude(a => a!.Building)
				.Include(c => c.Apartment)
					.ThenInclude(a => a!.User)
				.Where(c => c.Apartment!.BuildingId == buildingId)
				.OrderByDescending(c => c.CreatedDate)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

		public async Task<List<Complaint>> GetActiveForBuildingAsync(Guid buildingId)
		{
			return await _context.Complaints
				.AsNoTracking()
				.Where(c => c.Apartment!.BuildingId == buildingId &&
							c.Status != ComplaintStatus.Resolved &&
							c.Status != ComplaintStatus.Closed)
				.OrderByDescending(c => c.CreatedDate)
				.ToListAsync();
		}

		public async Task<List<Complaint>> GetByUserTelegramIdAsync(long telegramId)
		{
			return await _context.Complaints
				.AsNoTracking()
				.Include(c => c.Apartment)
					.ThenInclude(a => a!.Building)
				.Where(c => c.Apartment!.User!.TelegramId == telegramId)
				.OrderByDescending(c => c.CreatedDate)
				.ToListAsync();
		}

		public async Task ChangeStatusAsync(Guid complaintId, ComplaintStatus newStatus)
		{
			var complaint = await _context.Complaints.FindAsync(complaintId);
			if (complaint != null)
			{
				complaint.Status = newStatus;
				if (newStatus == ComplaintStatus.Resolved || newStatus == ComplaintStatus.Closed)
					complaint.ResolvedDate = DateTime.UtcNow;

				await _context.SaveChangesAsync();
			}
		}

		public async Task<List<Complaint>> GetUnprocessedAsync()
		{
			return await _context.Complaints
				.AsNoTracking()
				.Include(c => c.Apartment)
					.ThenInclude(a => a!.User)
				.Include(c => c.Apartment)
					.ThenInclude(a => a!.Building)
				.Where(c => c.Status == ComplaintStatus.New)
				.OrderBy(c => c.CreatedDate)
				.Take(50)
				.ToListAsync();
		}

		public async Task DeleteAsync(Guid id)
		{
			var complaint = await GetByIdAsync(id);
			if (complaint != null)
			{
				_context.Complaints.Remove(complaint);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<bool> ExistsAsync(Guid id)
		{
			return await _context.Complaints.AnyAsync(c => c.Id == id);
		}
	}
}
