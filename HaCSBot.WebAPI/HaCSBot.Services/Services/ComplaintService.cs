using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
	public class ComplaintService : IComplaintService
	{
		private readonly IComplaintRepository _repository;

		public ComplaintService(IComplaintRepository repository)
		{
			_repository = repository;
		}
    }
}
