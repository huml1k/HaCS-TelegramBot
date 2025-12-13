using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IApartmentRepository _apartmentRepository;

        public BuildingService(IBuildingRepository buildingRepository, IApartmentRepository apartmentRepository)
        {
            _buildingRepository = buildingRepository;
            _apartmentRepository = apartmentRepository;
        }

        public async Task<BuildingDto?> FindBuildingByAddressAsync(string fullAddress)
        {
            if (string.IsNullOrWhiteSpace(fullAddress))
                return null;

            // Приводим к нижнему регистру и убираем лишние пробелы для удобства парсинга
            var normalized = fullAddress.Trim().ToLowerInvariant();

            // Словарь: все возможные строковые представления типов улиц → enum
            var streetTypeMappings = new Dictionary<string, StreetsType>
            {
                // Полные названия
                { "улица", StreetsType.Улица },
                { "проспект", StreetsType.Проспект },
                { "переулок", StreetsType.Переулок },
                { "бульвар", StreetsType.Бульвар },
                { "площадь", StreetsType.Площадь },
                { "проезд", StreetsType.Проезд },
                { "шоссе", StreetsType.Шоссе },

                // Сокращения и альтернативы
                { "ул", StreetsType.Улица },
                { "ул.", StreetsType.Улица },
                { "пр-кт", StreetsType.Проспект },
                { "просп", StreetsType.Проспект },
                { "пр", StreetsType.Проспект },
                { "пер", StreetsType.Переулок },
                { "пер.", StreetsType.Переулок },
                { "б-р", StreetsType.Бульвар },
                { "бул", StreetsType.Бульвар },
                { "бульв", StreetsType.Бульвар },
                { "пл", StreetsType.Площадь },
                { "пл.", StreetsType.Площадь },
                { "пр-д", StreetsType.Проезд },
                { "проез", StreetsType.Проезд },
                { "ш", StreetsType.Шоссе },
                { "шос", StreetsType.Шоссе }
            };

            StreetsType? detectedType = null;
            string cleanedAddress = normalized;

            // Ищем самое длинное совпадение в начале строки (чтобы "проспект" не путался с "пр")
            string? matchedKey = null;
            foreach (var key in streetTypeMappings.Keys.OrderByDescending(k => k.Length))
            {
                if (normalized.StartsWith(key + " "))
                {
                    matchedKey = key;
                    detectedType = streetTypeMappings[key];
                    // Убираем тип улицы из строки
                    cleanedAddress = normalized.Substring(key.Length).TrimStart();
                    break;
                }
            }

            // Если тип не найден в начале — пробуем искать в любом месте (например, "Ленина ул. 25")
            if (detectedType == null)
            {
                foreach (var key in streetTypeMappings.Keys.OrderByDescending(k => k.Length))
                {
                    int index = normalized.IndexOf(" " + key + " ");
                    if (index == -1 && normalized.EndsWith(" " + key)) // на конце
                        index = normalized.Length - key.Length - 1;

                    if (index != -1)
                    {
                        matchedKey = key;
                        detectedType = streetTypeMappings[key];
                        // Убираем найденный тип улицы
                        cleanedAddress = normalized.Replace(key, "").Trim();
                        // Убираем лишние пробелы, которые могли остаться
                        cleanedAddress = System.Text.RegularExpressions.Regex.Replace(cleanedAddress, @"\s+", " ");
                        break;
                    }
                }
            }

            // Если тип улицы так и не найден — возвращаем null (или можно бросить исключение)
            if (detectedType == null)
                return null;

            // Теперь в cleanedAddress осталось что-то вроде: "ленина 25" или "ленина, 25" или "25 ленина"
            // Извлекаем номер дома — это обычно последнее слово, содержащее цифры, буквы (25а, 10к2 и т.д.)
            var numberMatch = System.Text.RegularExpressions.Regex.Match(cleanedAddress, @"\b(\d+[а-яА-Яa-zA-Z]?(?:/\d+[а-яА-Яa-zA-Z]?)?)\b");
            if (!numberMatch.Success)
                return null;

            string buildingNumber = numberMatch.Groups[1].Value.Trim();

            // Убираем номер дома из строки, чтобы получить чистое название улицы
            string streetNamePart = System.Text.RegularExpressions.Regex.Replace(cleanedAddress, @"\b\d+[а-яА-Яa-zA-Z]?(?:/\d+[а-яА-Яa-zA-Z]?)?\b", "")
                .Replace(",", "")
                .Trim();

            // Очищаем от лишних слов и пробелов
            streetNamePart = System.Text.RegularExpressions.Regex.Replace(streetNamePart, @"\s+", " ").Trim();

            if (string.IsNullOrEmpty(streetNamePart))
                return null;

            // Первая буква заглавная для красоты, но в БД будем искать в нижнем регистре
            string streetName = char.ToUpper(streetNamePart[0]) + streetNamePart.Substring(1);

            // Поиск в репозитории
            var building = await _buildingRepository.GetByFullAddressAsync(detectedType.Value, streetName, buildingNumber);

            if (building == null)
                return null;

            return new BuildingDto
            {
                Id = building.Id,
                FullAddress = $"{building.StreetType.GetDisplayName()} {building.StreetName}, {building.BuildingNumber}"
            };
        }

        public async Task<List<BuildingDto>> GetAdminBuildingsAsync(Guid adminUserId)
        {
            var buildings = await _buildingRepository.GetBuildingsByUserIdAsync(adminUserId);
            return buildings.Select(b => new BuildingDto
            {
                Id = b.Id,
                FullAddress = $"{b.StreetType} {b.StreetName}, {b.BuildingNumber}"
            }).ToList();
        }

        public async Task<List<ApartmentDto>> GetApartmentsInBuildingAsync(Guid buildingId)
        {
            var apartments = await _apartmentRepository.GetApartmentsByBuildingIdAsync(buildingId);
            return apartments.Select(a => new ApartmentDto
            {
                Id = a.Id,
                Number = a.ApartmentNumber,
                OwnerName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Свободна"
            }).ToList();
        }
    }

}
