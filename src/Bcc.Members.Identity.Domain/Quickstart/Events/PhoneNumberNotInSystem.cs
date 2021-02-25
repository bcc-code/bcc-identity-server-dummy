using IdentityServer4.Events;

namespace Bcc.Members.Identity.Domain.Quickstart.Events
{
    /// <summary>
    /// We don't have the mobile number registered in our system.
    /// </summary>
    public class PhoneNumberNotInSystem : Event
    {
        public long TelegramId;
        public string PhoneNumber;

        public PhoneNumberNotInSystem(long telegramId, string phoneNumber)
            : base("Error", nameof(PhoneNumberNotInSystem), EventTypes.Error, 12, $"Phone number {phoneNumber} for telegram user {telegramId} is not known in our system")
        {
            TelegramId = telegramId;
            PhoneNumber = phoneNumber;
        }
    }
}