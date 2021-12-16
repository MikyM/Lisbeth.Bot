using System.Text.Json;

namespace MikyM.Common.Application.CommandHandlers;

public abstract class CommandBase : ICommand
{
    public override string ToString()
        => JsonSerializer.Serialize(this);
}
