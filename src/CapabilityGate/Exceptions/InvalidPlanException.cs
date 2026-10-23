namespace CapabilityGate.Exceptions;

public sealed class InvalidPlanException : Exception
{
    public InvalidPlanException(string message) : base(message) { }
}
