namespace CapabilityGate.Execution;

using CapabilityGate.Interfaces;
using CapabilityGate.Models;

public sealed class QuarantineService
{
    private readonly IQuarantinedModel _model;

    public QuarantineService(IQuarantinedModel model)
    {
        _model = model;
    }

    public async Task<TaintedValue<string>> TransformAsync(
        TaintedValue<string> content,
        string task,
        string modelSourceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task);
        var output = await _model.TransformUntrustedAsync(content, task, cancellationToken).ConfigureAwait(false);
        var provenance = ProvenanceSet.QuarantinedOutput(content.Provenance, modelSourceId);
        return TaintedValue<string>.FromDerived(output, provenance);
    }
}
