namespace HaCSBot.Services.Enums
{
	public enum ConversationState
	{
		None,
		AwaitingFirstName,
		AwaitingLastName,
		AwaitingPhone,
        AwaitingTariffAddress,
        AwaitingComplaintApartment,     
        AwaitingComplaintCategory,      
        AwaitingComplaintDescription,   
        AwaitingComplaintAttachments,
        AwaitingMeterApartment,       
        AwaitingMeterType,            
        AwaitingMeterValue
    }
}
