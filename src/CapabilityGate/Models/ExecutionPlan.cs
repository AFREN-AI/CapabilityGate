namespace CapabilityGate.Models;

using CapabilityGate.Exceptions;

public sealed class ExecutionPlan
{
    public const int MaximumSteps = 64;

    private ExecutionPlan(IReadOnlyList<ToolCall> calls)
    {
        Calls = calls;
    }

    public IReadOnlyList<ToolCall> Calls { get; }

    public static ExecutionPlan Freeze(IEnumerable<ToolCall> proposedCalls)
    {
        ArgumentNullException.ThrowIfNull(proposedCalls);
        var calls = proposedCalls.ToArray();
        if (calls.Length == 0)
            throw new InvalidPlanException("A plan must contain at least one tool call.");
        if (calls.Length > MaximumSteps)
            throw new InvalidPlanException($"A plan cannot exceed {MaximumSteps} steps.");
        if (calls.Any(call => call is null))
            throw new InvalidPlanException("A plan cannot contain null calls.");
        return new ExecutionPlan(Array.AsReadOnly(calls));
    }
}
