namespace CapabilityGate.Models;

using System.Collections.Immutable;
using System.Text.Json;

public sealed class ToolCall
{
    public ToolCall(
        string toolName,
        IReadOnlyDictionary<string, TaintedValue<JsonElement>> arguments,
        ProvenanceSet controlProvenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(controlProvenance);
        ToolName = toolName;
        Arguments = arguments.ToImmutableDictionary(
            pair => pair.Key,
            pair => TaintedValue<JsonElement>.FromDerived(pair.Value.Value.Clone(), pair.Value.Provenance),
            StringComparer.Ordinal);
        ControlProvenance = controlProvenance;
    }

    public string ToolName { get; }
    public ImmutableDictionary<string, TaintedValue<JsonElement>> Arguments { get; }
    public ProvenanceSet ControlProvenance { get; }
}
