namespace CapabilityGate.Models;

using System.Collections.Immutable;
using System.Text.Json;

public sealed record ExecutionResult(
    ImmutableArray<ExecutionRecord> Records,
    ImmutableArray<TaintedValue<JsonElement>> Outputs);
