namespace CapabilityGate.Models;

public sealed record PolicyDecision(bool Allowed, string Reason);
