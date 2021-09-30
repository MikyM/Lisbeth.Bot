using System;

namespace MikyM.Common.Domain.Entities
{
    public enum AuditType
    {
        None = 0,
        Create = 1,
        Update = 2,
        Disable = 3,
        Delete = 4
    }

    public class Audit : EnvironmentSpecificEntity
    {
        public string UserId { get; set; }
        public string Type { get; set; }
        public string TableName { get; set; }
        public DateTime DateTime { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string AffectedColumns { get; set; }
        public string PrimaryKey { get; set; }
    }
}
