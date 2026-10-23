namespace CapabilityGate.Policies;

using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public static CapabilityPolicy FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var options = new JsonSerializerOptions { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
        var document = JsonSerializer.Deserialize<PolicyDocument>(json, options)
            ?? throw new JsonException("Policy document must be a JSON object.");
        if (document.GrantedCapabilities is null || document.Tools is null)
            throw new JsonException("Policy needs granted_capabilities and tools arrays.");
        var rules = document.Tools.Select(tool =>
        {
            if (tool.AllowedArgumentSources is null || tool.AllowedControlSources is null)
                throw new JsonException("Each tool needs allowed_argument_sources and allowed_control_sources.");
            var argumentSources = tool.AllowedArgumentSources.ToDictionary(
                pair => pair.Key,
                pair => ParseTrustClasses(pair.Value),
                StringComparer.Ordinal);
            return new CapabilityRule(
                tool.ToolName ?? throw new JsonException("tool_name is required."),
                tool.RequiredCapability ?? throw new JsonException("required_capability is required."),
                argumentSources,
                ParseTrustClasses(tool.AllowedControlSources));
        }).ToArray();
        return new CapabilityPolicy(rules, document.GrantedCapabilities);
    }

    private static TrustClass[] ParseTrustClasses(IEnumerable<string>? names)
    {
        if (names is null) throw new JsonException("Trust source arrays cannot be null.");
        return names.Select(name => Enum.TryParse<TrustClass>(name, true, out var value)
            ? value
            : throw new JsonException($"Unknown trust class '{name}'.")).ToArray();
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

    private sealed record PolicyDocument(
        [property: JsonPropertyName("granted_capabilities")] string[]? GrantedCapabilities,
        [property: JsonPropertyName("tools")] ToolDocument[]? Tools);

    private sealed record ToolDocument(
        [property: JsonPropertyName("tool_name")] string? ToolName,
        [property: JsonPropertyName("required_capability")] string? RequiredCapability,
        [property: JsonPropertyName("allowed_argument_sources")] Dictionary<string, string[]>? AllowedArgumentSources,
        [property: JsonPropertyName("allowed_control_sources")] string[]? AllowedControlSources);
}
