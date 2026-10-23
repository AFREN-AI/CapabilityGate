namespace CapabilityGate.Exceptions;

using CapabilityGate.Models;

public sealed class ToolAuthorizationException : Exception
{
    public ToolAuthorizationException(string toolName, string reason, IReadOnlyList<ExecutionRecord> records)
        : base($"Tool '{toolName}' was denied: {reason}")
    {
        ToolName = toolName;
        Reason = reason;
        Records = records;
    }

    public string ToolName { get; }
    public string Reason { get; }
    public IReadOnlyList<ExecutionRecord> Records { get; }
}
