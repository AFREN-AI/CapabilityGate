namespace CapabilityGate.Execution;

using System.Collections.Immutable;
using System.Text.Json;
using CapabilityGate.Exceptions;
using CapabilityGate.Interfaces;
using CapabilityGate.Models;
using CapabilityGate.Policies;

public sealed class ExecutionEngine
{
    private readonly CapabilityPolicy _policy;
    private readonly IToolExecutor _executor;

    public ExecutionEngine(CapabilityPolicy policy, IToolExecutor executor)
    {
        _policy = policy;
        _executor = executor;
    }

    public async Task<ExecutionResult> ExecutePlanAsync(ExecutionPlan plan, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var records = ImmutableArray.CreateBuilder<ExecutionRecord>();
        var outputs = ImmutableArray.CreateBuilder<TaintedValue<JsonElement>>();
        foreach (var call in plan.Calls)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var decisionId = Guid.NewGuid().ToString("N");
            var decision = _policy.Authorize(call);
            records.Add(new ExecutionRecord(call.ToolName, decisionId, decision.Allowed, decision.Reason));
            if (!decision.Allowed)
                throw new ToolAuthorizationException(call.ToolName, decision.Reason, records.ToImmutable());

            var ruleCapability = _policy.RequiredCapabilityFor(call.ToolName);
            var authorized = new AuthorizedToolCall(call, ruleCapability, decisionId);
            var output = await _executor.ExecuteAsync(authorized, cancellationToken).ConfigureAwait(false);
            var provenanceParts = new[] { call.ControlProvenance, ProvenanceSet.ExternalContent($"tool:{call.ToolName}:{decisionId}") }
                .Concat(call.Arguments.Values.Select(argument => argument.Provenance))
                .ToArray();
            var provenance = ProvenanceSet.Combine(provenanceParts);
            outputs.Add(TaintedValue<JsonElement>.FromDerived(output.Clone(), provenance));
        }
        return new ExecutionResult(records.ToImmutable(), outputs.ToImmutable());
    }
}
