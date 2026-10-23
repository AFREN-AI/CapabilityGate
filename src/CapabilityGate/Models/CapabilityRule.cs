namespace CapabilityGate.Models;

using System.Collections.Immutable;

public sealed class CapabilityRule
{
    public CapabilityRule(
        string toolName,
        string requiredCapability,
        IReadOnlyDictionary<string, TrustClass[]> allowedArgumentSources,
        IEnumerable<TrustClass> allowedControlSources)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredCapability);
        ToolName = toolName;
        RequiredCapability = requiredCapability;
        AllowedArgumentSources = allowedArgumentSources.ToImmutableDictionary(
            pair => pair.Key,
            pair => pair.Value.ToImmutableHashSet(),
            StringComparer.Ordinal);
        AllowedControlSources = allowedControlSources.ToImmutableHashSet();
    }

    public string ToolName { get; }
    public string RequiredCapability { get; }
    public ImmutableDictionary<string, ImmutableHashSet<TrustClass>> AllowedArgumentSources { get; }
    public ImmutableHashSet<TrustClass> AllowedControlSources { get; }
}
