namespace CapabilityGate.Models;

using System.Collections.Immutable;
using System.Text.Json;

public sealed class AuthorizedToolCall
{
    internal AuthorizedToolCall(ToolCall call, string capability, string decisionId)
    {
        ToolName = call.ToolName;
        Capability = capability;
        DecisionId = decisionId;
        Arguments = call.Arguments.ToImmutableDictionary(
            pair => pair.Key,
            pair => pair.Value.Value.Clone(),
            StringComparer.Ordinal);
    }

    public string ToolName { get; }
    public string Capability { get; }
    public string DecisionId { get; }
    public ImmutableDictionary<string, JsonElement> Arguments { get; }
}
