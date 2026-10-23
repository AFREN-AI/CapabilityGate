namespace CapabilityGate.Interfaces;

using System.Text.Json;
using CapabilityGate.Models;

/// <summary>Executes only calls already authorized by the gate.</summary>
public interface IToolExecutor
{
    Task<JsonElement> ExecuteAsync(AuthorizedToolCall call, CancellationToken cancellationToken);
}
