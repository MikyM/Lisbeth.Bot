using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lisbeth.Bot.DataAccessLayer.Configurations;

public class DateTimeKindConverter : ValueConverter<DateTime?, DateTime?>
{
    public DateTimeKindConverter() 
        : base(x => !x.HasValue 
            ? null 
            : x.Value.Kind == DateTimeKind.Utc 
                ? DateTime.SpecifyKind(x.Value, DateTimeKind.Unspecified)
                : x.Value, x => x)
    {
    }
}
