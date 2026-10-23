namespace CapabilityGate.Policies;

using System.Collections.Immutable;
using CapabilityGate.Models;

public sealed class CapabilityPolicy
{
    private readonly ImmutableDictionary<string, CapabilityRule> _rules;
    private readonly ImmutableHashSet<string> _grantedCapabilities;

    public CapabilityPolicy(IEnumerable<CapabilityRule> rules, IEnumerable<string> grantedCapabilities)
    {
        _rules = rules.ToImmutableDictionary(rule => rule.ToolName, StringComparer.Ordinal);
        _grantedCapabilities = grantedCapabilities.ToImmutableHashSet(StringComparer.Ordinal);
    }

    public PolicyDecision Authorize(ToolCall call)
    {
        if (!_rules.TryGetValue(call.ToolName, out var rule))
            return new PolicyDecision(false, "tool is not registered in policy");
        if (!_grantedCapabilities.Contains(rule.RequiredCapability))
            return new PolicyDecision(false, "required capability was not granted by the host");
        foreach (var argument in call.Arguments)
        {
            if (!rule.AllowedArgumentSources.TryGetValue(argument.Key, out var allowedSources))
                return new PolicyDecision(false, $"argument '{argument.Key}' is not declared in the tool policy");
            if (argument.Value.Provenance.Tags.Count == 0)
                return new PolicyDecision(false, $"argument '{argument.Key}' has no provenance");
            if (argument.Value.Provenance.Tags.Any(tag => !allowedSources.Contains(tag.Trust)))
                return new PolicyDecision(false, $"argument '{argument.Key}' contains a disallowed provenance source");
        }
        if (call.ControlProvenance.Tags.Count == 0 ||
            call.ControlProvenance.Tags.Any(tag => !rule.AllowedControlSources.Contains(tag.Trust)))
            return new PolicyDecision(false, "control flow contains a disallowed or missing provenance source");
        return new PolicyDecision(true, "allowed by explicit capability policy");
    }

    internal string RequiredCapabilityFor(string toolName) => _rules[toolName].RequiredCapability;
}
