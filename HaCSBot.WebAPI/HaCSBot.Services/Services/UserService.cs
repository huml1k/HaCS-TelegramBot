using AutoMapper;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository,
            IApartmentRepository apartmentRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _apartmentRepository = apartmentRepository;
        }

        public async Task<User?> GetCurrentUserAsync(long telegramId)
        {
            return await _userRepository.GetByTelegramIdAsync(telegramId);
        }

        public async Task<bool> IsUserAuthorizedAsync(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            return user?.IsAuthorizedInBot ?? false;
        }

        //public async Task<AuthorizationResult> RegisterOrLoginAsync(UserRegistrationDto dto)
        //{
        //    // Логика регистрации/логина: поиск по телефону, создание пользователя, привязка TelegramId
        //    var existingUser = await _userRepository.GetByPhoneAsync(dto.Phone);
        //    if (existingUser != null)
        //    {
        //        // Логин: обновить TelegramId и авторизацию
        //        existingUser.TelegramId = dto.TelegramId;
        //        existingUser.IsAuthorizedInBot = true;
        //        existingUser.LastAuthorizationDate = DateTime.UtcNow;
        //        await _userRepository.UpdateAsync(existingUser);
        //        return new AuthorizationResult { Success = true, UserId = existingUser.Id };
        //    }

        //    // Регистрация нового
        //    var newUser = new User
        //    {
        //        FirstName = dto.FirstName,
        //        LastName = dto.LastName,
        //        MiddleName = dto.MiddleName,
        //        Phone = dto.Phone,
        //        TelegramId = dto.TelegramId,
        //        Role = Roles.Resident,
        //        CreatedDate = DateTime.UtcNow,
        //        IsAuthorizedInBot = true,
        //        LastAuthorizationDate = DateTime.UtcNow
        //    };
        //    await _userRepository.AddAsync(newUser);
        //    return new AuthorizationResult { Success = true, UserId = newUser.Id };
        //}

        public async Task<List<ApartmentInfoDto>> GetUserApartmentsAsync(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user == null) return new List<ApartmentInfoDto>();

            var apartments = await _apartmentRepository.GetByUserIdAsync(user.Id);
            return apartments.Select(a => new ApartmentInfoDto
            {
                Id = a.Id,
                Number = a.ApartmentNumber,
                BuildingAddress = $"{a.Building.StreetType} {a.Building.StreetName}, {a.Building.BuildingNumber}"
            }).ToList();
        }

        public async Task<UserProfileDto> GetProfileAsync(long telegramId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user == null) throw new InvalidOperationException("User not found");

            return new UserProfileDto
            {
                FullName = $"{user.FirstName} {user.LastName} {user.MiddleName}",
                Phone = user.Phone,
                Role = user.Role,
                Apartments = user.Apartments.Select(a => a.ApartmentNumber).ToList()
            };
        }

        public async Task ChangeApartmentAsync(long telegramId, Guid apartmentId)
        {
            var user = await _userRepository.GetByTelegramIdAsync(telegramId);
            if (user == null) throw new InvalidOperationException("User not found");

            var apartment = await _apartmentRepository.GetByIdAsync(apartmentId);
            if (apartment == null || apartment.UserId != user.Id) throw new InvalidOperationException("Apartment not found or not owned");

            // Логика переключения, если нужно (например, установить активную квартиру в user, если добавлено поле)
            // Пока просто проверка
            await Task.CompletedTask;
        }

        public async Task<User?> FindByPersonalDataAsync(string firstName, string lastName, string phone)
        {
            var normalizedPhone = NormalizePhone(phone);

            var allUsers = await _userRepository.GetAllAsync();

            return allUsers.FirstOrDefault(u =>
                u.FirstName.Trim() == firstName.Trim() &&
                u.LastName.Trim() == lastName.Trim() &&
                NormalizePhone(u.Phone) == normalizedPhone);
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "";
            return phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        }

        public async Task<User?> GetByTelegramIdAsync(long telegramId)
        {
            var allUsers = await _userRepository.GetAllAsync();
            return allUsers.FirstOrDefault(u => u.TelegramId == telegramId && u.IsAuthorizedInBot);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }
    }
}

