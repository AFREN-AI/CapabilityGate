namespace CapabilityGate.Models;

public sealed record ExecutionRecord(string ToolName, string DecisionId, bool Allowed, string Reason);
