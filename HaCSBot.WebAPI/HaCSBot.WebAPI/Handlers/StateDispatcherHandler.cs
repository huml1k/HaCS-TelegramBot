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
        private readonly AdminPanelHandler _adminPanelHandler;
        private readonly NotificationHandler _notificationHandler;
        private readonly IMapper _mapper;

        public StateDispatcherHandler(
            ITelegramBotClient bot,
            IUserStateService userState,
            IUserService userService,
            MeterReadingHandler meterReadingHandler,
            MainMenuHandler mainMenuHandler,
            ComplaintHandler complaintHandler,
            TariffHandler tariffHandler, // Добавляем в конструктор
            AdminPanelHandler adminPanelHandler,
            NotificationHandler notificationHandler,
            IMapper mapper)
        {
            _bot = bot;
            _userState = userState;
            _userService = userService;
            _meterReadingHandler = meterReadingHandler;
            _mainMenuHandler = mainMenuHandler;
            _complaintHandler = complaintHandler;
            _tariffHandler = tariffHandler;
            _adminPanelHandler = adminPanelHandler;
            _notificationHandler = notificationHandler;
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

				case ConversationState.AwaitingComplaintPhoto:
					await _complaintHandler.HandleComplaintAttachments(msg, userProfileDto);
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
                case ConversationState.AwaitingTariffApartment:
                    await _tariffHandler.HandleTariffApartmentSelection(msg, userProfileDto);
                    break;
                case ConversationState.AdminNotificationRecipient:
                    await _notificationHandler.HandleRecipientSelection(msg, userProfileDto);
                    break;
                case ConversationState.AdminNotificationBuilding:
                    await _notificationHandler.HandleBuildingSelection(msg, userProfileDto);
                    break;
                case ConversationState.AdminNotificationType:
                    await _notificationHandler.HandleNotificationType(msg, userProfileDto);
                    break;
                case ConversationState.AdminNotificationMessage:
                    await _notificationHandler.HandleNotificationMessage(msg, userProfileDto);
                    break;
                case ConversationState.AdminNotificationTitle:
                    await _notificationHandler.HandleNotificationTitle(msg, userProfileDto);
                    break;
                case ConversationState.AdminNotificationScheduled:
                    await _notificationHandler.HandleNotificationScheduledOrAttachments(msg, userProfileDto);
                    break;
                case ConversationState.AdminNotificationScheduledDate:
                    await _notificationHandler.HandleNotificationScheduledDate(msg, userProfileDto);
                    break;
                case ConversationState.AdminViewComplaintsList:
                    await _complaintHandler.HandleAdminComplaintSelection(msg, userProfileDto);
                    break;
                case ConversationState.AdminChangeComplaintStatus:
                    await _complaintHandler.HandleAdminComplaintStatusChange(msg, userProfileDto);
                    break;
            }
        }
    }
}
