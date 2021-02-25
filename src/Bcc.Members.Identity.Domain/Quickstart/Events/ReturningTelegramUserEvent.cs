using IdentityServer4.Events;

namespace Bcc.Members.Identity.Domain.Quickstart.Events
{
    public class ReturningTelegramUserEvent : Event
    {
        public int PersonId;

        public ReturningTelegramUserEvent(int personId)
            : base("Info", nameof(ReturningTelegramUserEvent), EventTypes.Information, 11, $"Person {personId} is a returning Telegram user that has connected before")
        {
            PersonId = personId;
        }
    }
}