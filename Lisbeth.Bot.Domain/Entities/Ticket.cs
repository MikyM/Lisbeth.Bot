using System;
using System.ComponentModel.DataAnnotations.Schema;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Ticket : DiscordAggregateRootEntity
    {
        public long ChannelId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime ReopenedOn { get; set; }
        public DateTime ClosedOn { get; set; }
        public long ClosedBy { get; set; }
        public long ReopenedBy { get; set; }
        public long MessageOpenId { get; set; }
        public long MessageCloseId { get; set; } = 0;
        public long MessageReopenId { get; set; } = 0;
        public bool IsPrivate { get; set; } = false;

        [NotMapped]
        public ulong _ChannelId
        {
            get
            {
                unchecked
                {
                    return (ulong)ChannelId;
                }
            }
            set
            {
                unchecked
                {
                    ChannelId = (long)value;
                }
            }
        }

        [NotMapped]
        public ulong _ReopenedBy
        {
            get
            {
                unchecked
                {
                    return (ulong)ReopenedBy;
                }
            }
            set
            {
                unchecked
                {
                    ReopenedBy = (long)value;
                }
            }
        }

        [NotMapped]
        public ulong _ClosedBy
        {
            get
            {
                unchecked
                {
                    return (ulong)ClosedBy;
                }
            }
            set
            {
                unchecked
                {
                    ClosedBy = (long)value;
                }
            }
        }

        [NotMapped]
        public ulong _MessageCloseId
        {
            get
            {
                unchecked
                {
                    return (ulong)MessageCloseId;
                }
            }
            set
            {
                unchecked
                {
                    MessageCloseId = (long)value;
                }
            }
        }

        [NotMapped]
        public ulong _MessageOpenId
        {
            get
            {
                unchecked
                {
                    return (ulong)MessageOpenId;
                }
            }
            set
            {
                unchecked
                {
                    MessageOpenId = (long)value;
                }
            }
        }

        [NotMapped]
        public ulong _MessageReopenId
        {
            get
            {
                unchecked
                {
                    return (ulong)MessageReopenId;
                }
            }
            set
            {
                unchecked
                {
                    MessageReopenId = (long)value;
                }
            }
        }
    }
}
