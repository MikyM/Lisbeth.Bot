using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.Domain.Entities
{
    public class AuditLog : EnvironmentSpecificEntity
    {
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }
}
