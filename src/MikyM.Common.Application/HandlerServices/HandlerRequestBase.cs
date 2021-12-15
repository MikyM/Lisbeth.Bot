using System.Text.Json;

namespace MikyM.Common.Application.HandlerServices;

public abstract class HandlerRequestBase : IHandlerRequest
{
    public override string ToString()
        => JsonSerializer.Serialize(this);
}
