namespace CapabilityGate.Models;

public sealed class TaintedValue<T>
{
    private TaintedValue(T value, ProvenanceSet provenance)
    {
        Value = value;
        Provenance = provenance;
    }

    public T Value { get; }
    public ProvenanceSet Provenance { get; }

    public static TaintedValue<T> FromUserIntent(T value, string sourceId) =>
        new(value, ProvenanceSet.UserIntent(sourceId));

    public static TaintedValue<T> FromHostPolicy(T value, string sourceId) =>
        new(value, ProvenanceSet.HostPolicy(sourceId));

    public static TaintedValue<T> FromExternalContent(T value, string sourceId) =>
        new(value, ProvenanceSet.ExternalContent(sourceId));

    public static TaintedValue<T> Derived(T value, string operationId, params ProvenanceSet[] inputSources)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        ArgumentNullException.ThrowIfNull(inputSources);
        if (inputSources.Length == 0)
            throw new ArgumentException("At least one source is required for a derived value.", nameof(inputSources));
        var sources = inputSources.Append(ProvenanceSet.DerivedValue(operationId)).ToArray();
        return new TaintedValue<T>(value, ProvenanceSet.Combine(sources));
    }

    public TaintedValue<TResult> Map<TResult>(Func<T, TResult> transform, string operationId)
    {
        ArgumentNullException.ThrowIfNull(transform);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        var derived = ProvenanceSet.Combine(Provenance, ProvenanceSet.DerivedValue(operationId));
        return TaintedValue<TResult>.FromDerived(transform(Value), derived);
    }

    internal static TaintedValue<T> FromDerived(T value, ProvenanceSet provenance) => new(value, provenance);
}
