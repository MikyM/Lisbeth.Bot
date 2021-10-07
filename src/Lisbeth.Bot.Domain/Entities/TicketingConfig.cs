using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lisbeth.Bot.Domain.Entities
{
    public class TicketingConfig
    {
        public long Id { get; set; }
        public ulong? LogChannelId { get; set; }
        public long LastTicketId { get; set; }
        public ulong ClosedCategoryId { get; set; }
        public ulong OpenedCategoryId { get; set; }
        public TimeSpan? CleanAfter { get; set; }
        public TimeSpan? CloseAfter { get; set; }
        public string OpenedNamePrefix { get; set; } = "ticket";
        public string ClosedNamePrefix { get; set; } = "closed";
        public string WelcomeMessage { get; set; } = "@ownerMention@ please be patient, support will be with you shortly!";
        public string TicketCenterMessage { get; set; } = "\n\nClick on the button below to create a private ticket between the staff members and you. Explain your issue, and a staff member will be here to help you shortly after. Please note it may take up to 48 hours for an answer.";

        public string WhenToUseCenterMessage { get; set; } =
            "This is only in the case you need private help.\nFor normal questions please refer to <#869598053139628062>.\nFor reporting bugs please refer to <#875033712918671411>." +
            "\n\n**Note: Creating tickets for no purpose is punishable.**";

        public string AdditionalInformationCenterMessage { get; set; } =
            "You might want to check the following links, which might answer your questions:" +
            "\n- Gameplay and Technical FAQs: https://eclipse-flyff.com/General/FAQ" +
            "\n- Official Guides: https://eclipse-flyff.com/Guide" +
            "\n- Community Guides: https://eclipse-flyff.com/Guide/Community";

        public long GuildId { get; set; }
        public Guild Guild { get; set; }

        [NotMapped]
        public bool ShouldAutoClean => CleanAfter.HasValue;
        [NotMapped]
        public bool ShouldAutoClose => CloseAfter.HasValue;
    }
}
