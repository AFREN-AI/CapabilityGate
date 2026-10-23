namespace CapabilityGate.Tests;

using System.Text.Json;
using CapabilityGate.Exceptions;
using CapabilityGate.Execution;
using CapabilityGate.Interfaces;
using CapabilityGate.Models;
using CapabilityGate.Policies;
using Xunit;

public sealed class ExecutionTests
{
    [Fact]
    public async Task UnknownToolsAreDeniedWithoutCallingExecutor()
    {
        var executor = new RecordingExecutor();
        var engine = new ExecutionEngine(new CapabilityPolicy([], []), executor);
        var call = MakeCall("file.write", TrustClass.UserIntent);

        var exception = await Assert.ThrowsAsync<ToolAuthorizationException>(
            () => engine.ExecutePlanAsync(ExecutionPlan.Freeze([call])));

        Assert.Single(exception.Records);
        Assert.False(exception.Records[0].Allowed);
        Assert.Empty(executor.Calls);
    }

    [Fact]
    public async Task UntrustedArgumentsAreDeniedByDefault()
    {
        var rule = new CapabilityRule("mail.send", "mail.send", new Dictionary<string, TrustClass[]>(), [TrustClass.UserIntent]);
        var policy = new CapabilityPolicy([rule], ["mail.send"]);
        var executor = new RecordingExecutor();
        var engine = new ExecutionEngine(policy, executor);
        var call = MakeCall("mail.send", TrustClass.ExternalContent);

        await Assert.ThrowsAsync<ToolAuthorizationException>(
            () => engine.ExecutePlanAsync(ExecutionPlan.Freeze([call])));
        Assert.Empty(executor.Calls);
    }

    [Fact]
    public async Task ExplicitlyAllowedCallExecutesAndOutputRetainsInputProvenance()
    {
        var rule = new CapabilityRule(
            "search.read",
            "search.read",
            new Dictionary<string, TrustClass[]> { ["query"] = [TrustClass.UserIntent] },
            [TrustClass.UserIntent]);
        var policy = new CapabilityPolicy([rule], ["search.read"]);
        var executor = new RecordingExecutor();
        var engine = new ExecutionEngine(policy, executor);
        var result = await engine.ExecutePlanAsync(ExecutionPlan.Freeze([MakeCall("search.read", TrustClass.UserIntent)]));

        Assert.Single(executor.Calls);
        Assert.Equal("search.read", executor.Calls[0].Capability);
        Assert.Single(result.Records);
        Assert.True(result.Records[0].Allowed);
        Assert.Contains(result.Outputs[0].Provenance.Tags, tag => tag.Trust == TrustClass.UserIntent);
        Assert.Contains(result.Outputs[0].Provenance.Tags, tag => tag.Trust == TrustClass.ExternalContent);
    }

    [Fact]
    public async Task UntrustedControlFlowIsDeniedEvenWithTrustedArguments()
    {
        var rule = new CapabilityRule(
            "payment.refund",
            "payment.refund",
            new Dictionary<string, TrustClass[]>(),
            [TrustClass.UserIntent]);
        var policy = new CapabilityPolicy([rule], ["payment.refund"]);
        var executor = new RecordingExecutor();
        var call = new ToolCall(
            "payment.refund",
            new Dictionary<string, TaintedValue<JsonElement>>(),
            ProvenanceSet.ExternalContent("retrieved-page"));
        var engine = new ExecutionEngine(policy, executor);

        await Assert.ThrowsAsync<ToolAuthorizationException>(
            () => engine.ExecutePlanAsync(ExecutionPlan.Freeze([call])));
        Assert.Empty(executor.Calls);
    }

    [Fact]
    public void OversizedPlansAreRejected()
    {
        var calls = Enumerable.Range(0, ExecutionPlan.MaximumSteps + 1)
            .Select(_ => MakeCall("noop", TrustClass.UserIntent));

        Assert.Throws<InvalidPlanException>(() => ExecutionPlan.Freeze(calls));
    }

    private static ToolCall MakeCall(string name, TrustClass trust)
    {
        using var document = JsonDocument.Parse("\"query\"");
        var value = trust == TrustClass.UserIntent
            ? TaintedValue<JsonElement>.FromUserIntent(document.RootElement.Clone(), "request")
            : TaintedValue<JsonElement>.FromExternalContent(document.RootElement.Clone(), "retrieved");
        return new ToolCall(
            name,
            new Dictionary<string, TaintedValue<JsonElement>> { ["query"] = value },
            ProvenanceSet.UserIntent("request"));
    }

    private sealed class RecordingExecutor : IToolExecutor
    {
        public List<AuthorizedToolCall> Calls { get; } = [];

        public Task<JsonElement> ExecuteAsync(AuthorizedToolCall call, CancellationToken cancellationToken)
        {
            Calls.Add(call);
            using var result = JsonDocument.Parse("\"ok\"");
            return Task.FromResult(result.RootElement.Clone());
        }
    }
}
