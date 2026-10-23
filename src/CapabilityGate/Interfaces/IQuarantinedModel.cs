namespace CapabilityGate.Interfaces;

using CapabilityGate.Models;

/// <summary>A model endpoint with no tool registry or action-execution capability.</summary>
public interface IQuarantinedModel
{
    Task<string> TransformUntrustedAsync(TaintedValue<string> content, string task, CancellationToken cancellationToken);
}
