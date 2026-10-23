namespace CapabilityGate.Tests;

using CapabilityGate.Models;
using Xunit;

public sealed class ProvenanceTests
{
    [Fact]
    public void MapPreservesOriginAndAddsTransformationSource()
    {
        var source = TaintedValue<string>.FromExternalContent("external text", "web:1");
        var mapped = source.Map(value => value.ToUpperInvariant(), "uppercase");

        Assert.Contains(mapped.Provenance.Tags, tag => tag.Trust == TrustClass.ExternalContent && tag.SourceId == "web:1");
        Assert.Contains(mapped.Provenance.Tags, tag => tag.Trust == TrustClass.DerivedValue && tag.SourceId == "transform:uppercase");
    }

    [Fact]
    public void CombinePreservesEachSource()
    {
        var combined = ProvenanceSet.Combine(
            ProvenanceSet.UserIntent("request"),
            ProvenanceSet.ExternalContent("document"));

        Assert.Contains(combined.Tags, tag => tag.Trust == TrustClass.UserIntent);
        Assert.Contains(combined.Tags, tag => tag.Trust == TrustClass.ExternalContent);
    }

    [Fact]
    public void DerivedValueCanJoinMultipleInputSources()
    {
        var value = TaintedValue<string>.Derived(
            "joined",
            "join",
            ProvenanceSet.UserIntent("request"),
            ProvenanceSet.ExternalContent("document"));

        Assert.Equal(3, value.Provenance.Tags.Count);
    }
}
