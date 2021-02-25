using IdentityServer4.Events;

namespace Bcc.Members.Identity.Domain.Quickstart.Events
{
    public class AddTelegramIdToMemberEvent : Event
    {
        public int PersonId;

        public AddTelegramIdToMemberEvent(int personId)
            : base("Info", nameof(AddTelegramIdToMemberEvent), EventTypes.Information, 10, $"Added TelegramId for person {personId}")
        {
            PersonId = personId;
        }
    }
}
