using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public sealed class TicketingConfig : SnowflakeEntity
    {
        public ulong? LogChannelId { get; set; }
        public long LastTicketId { get; set; }
        public ulong ClosedCategoryId { get; set; }
        public ulong OpenedCategoryId { get; set; }
        public TimeSpan? CleanAfter { get; set; }
        public TimeSpan? CloseAfter { get; set; }
        public string OpenedNamePrefix { get; set; } = "ticket";
        public string ClosedNamePrefix { get; set; } = "closed";

        public string BaseWelcomeMessage { get; set; } =
            "@ownerMention@ please be patient, support will be with you shortly!";

        public EmbedConfig WelcomeEmbedConfig { get; set; }
        public long? WelcomeEmbedConfigId { get; set; }

        public string BaseCenterMessage { get; set; } =
            "\n\nClick on the button below to create a private ticket between the staff members and you. Explain your issue, and a staff member will be here to help you shortly after. Please note it may take up to 48 hours for an answer.";

        public EmbedConfig CenterEmbedConfig { get; set; }
        public long? CenterEmbedConfigId { get; set; }

        public ulong GuildId { get; set; }
        public Guild Guild { get; set; }

        [NotMapped] public bool ShouldAutoClean => CleanAfter.HasValue;

        [NotMapped] public bool ShouldAutoClose => CloseAfter.HasValue;
    }
}