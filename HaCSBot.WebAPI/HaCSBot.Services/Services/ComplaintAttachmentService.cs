using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class ComplaintAttachmentService : IComplaintAttachmentService
    {
        private readonly IComplaintAttachmentRepository _repository;

        public ComplaintAttachmentService(IComplaintAttachmentRepository repository)
        {
            _repository = repository;
        }
    }
}
