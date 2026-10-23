namespace CapabilityGate.Models;

using System.Collections.Immutable;

public sealed class ProvenanceSet
{
    private readonly ImmutableHashSet<ProvenanceTag> _tags;

    private ProvenanceSet(ImmutableHashSet<ProvenanceTag> tags)
    {
        _tags = tags;
    }

    public IReadOnlySet<ProvenanceTag> Tags => _tags;

    public static ProvenanceSet UserIntent(string sourceId) => Single(TrustClass.UserIntent, sourceId);
    public static ProvenanceSet HostPolicy(string sourceId) => Single(TrustClass.HostPolicy, sourceId);
    public static ProvenanceSet ExternalContent(string sourceId) => Single(TrustClass.ExternalContent, sourceId);
    public static ProvenanceSet DerivedValue(string operationId) => Single(TrustClass.DerivedValue, $"transform:{operationId}");

    public static ProvenanceSet Combine(params ProvenanceSet[] sets)
    {
        var result = ImmutableHashSet<ProvenanceTag>.Empty;
        foreach (var set in sets)
        {
            ArgumentNullException.ThrowIfNull(set);
            result = result.Union(set._tags);
        }
        return new ProvenanceSet(result);
    }

    internal static ProvenanceSet QuarantinedOutput(ProvenanceSet input, string sourceId) =>
        Combine(input, Single(TrustClass.QuarantinedModelOutput, sourceId));

    private static ProvenanceSet Single(TrustClass trust, string sourceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);
        return new ProvenanceSet(ImmutableHashSet.Create(new ProvenanceTag(trust, sourceId)));
    }
}
