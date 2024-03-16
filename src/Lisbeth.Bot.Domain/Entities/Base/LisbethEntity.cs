using System;
using DataExplorer.Abstractions.Entities;
using DataExplorer.Entities;

namespace Lisbeth.Bot.Domain.Entities.Base;

public abstract class LisbethEntity : SnowflakeEntity, ICreatedAt, IUpdatedAt, IDisableable
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDisabled { get; set; }
}
