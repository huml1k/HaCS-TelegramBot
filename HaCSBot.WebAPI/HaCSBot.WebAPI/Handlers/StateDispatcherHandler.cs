using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.Services.Enums;
using HaCSBot.Services.Services.Extensions;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HaCSBot.WebAPI.Handlers
{
	public class StateDispatcherHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly IUserStateService _userState;
        private readonly IUserService _userService;
        private readonly MeterReadingHandler _meterReadingHandler;
        private readonly MainMenuHandler _mainMenuHandler;
        private readonly ComplaintHandler _complaintHandler;
        private readonly TariffHandler _tariffHandler; // Добавляем TariffHandler
        private readonly IMapper _mapper;

        public StateDispatcherHandler(
            ITelegramBotClient bot,
            IUserStateService userState,
            IUserService userService,
            MeterReadingHandler meterReadingHandler,
            MainMenuHandler mainMenuHandler,
            ComplaintHandler complaintHandler,
            TariffHandler tariffHandler, // Добавляем в конструктор
            IMapper mapper)
        {
            _bot = bot;
            _userState = userState;
            _userService = userService;
            _meterReadingHandler = meterReadingHandler;
            _mainMenuHandler = mainMenuHandler;
            _complaintHandler = complaintHandler;
            _tariffHandler = tariffHandler;
            _mapper = mapper;
        }

        public async Task HandleStateInput(Message msg, ConversationState state)
        {
            long userId = msg.From!.Id;
            long chatId = msg.Chat.Id;
            string? text = msg.Text?.Trim();
            var dbUser = await _userService.GetCurrentUserAsync(userId);
            var userProfileDto = await _userService.GetProfileAsync(userId);

            if (dbUser == null)
            {
                await _mainMenuHandler.SendRegistrationButton(chatId);
                return;
            }

            var userDto = _mapper.Map<UserDto>(dbUser);

            switch (state)
            {
                case ConversationState.AwaitingComplaintApartment:
                    await _complaintHandler.HandleComplaintApartmentSelection(msg, userProfileDto);
                    break;
                    //вызывает метод костыль
                case ConversationState.AwaitingComplaintPhoto:
                    await _complaintHandler.HandleComplaintSendAnswer(msg, userProfileDto);
                    break;

                case ConversationState.AwaitingComplaintCategory:
                    await _complaintHandler.HandleComplaintCategorySelection(msg, userProfileDto);
                    break;

                case ConversationState.AwaitingComplaintDescription:
                    await _complaintHandler.HandleComplaintDescription(msg, userProfileDto);
                    break;

                case ConversationState.AwaitingMeterApartment:
                    await _meterReadingHandler.HandleMeterApartmentSelection(msg, userProfileDto);
                    break;

                case ConversationState.AwaitingMeterType:
                    await _meterReadingHandler.HandleMeterTypeSelection(msg, userProfileDto);
                    break;

                case ConversationState.AwaitingMeterValue:
                    await _meterReadingHandler.HandleMeterValueInput(msg, userProfileDto);
                    break;

                // Обработка тарифов делегируется TariffHandler
                case ConversationState.AwaitingTariffApartment:
                    await _tariffHandler.HandleTariffApartmentSelection(msg, userProfileDto);
                    break;

                case ConversationState.AwaitingTariffAddress:
                    await _tariffHandler.HandleTariffAddressInput(msg, userProfileDto);
                    break;
            }
        }
    }
}
