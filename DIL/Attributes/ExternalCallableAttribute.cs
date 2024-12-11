namespace DIL.Attributes
{
    /// <summary>
    /// Indicates that a class, property, or method can be called externally by the interpreter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ExternalCallableAttribute : Attribute
    {
        public string? Alias { get; } // Optional alias for external call.

        public ExternalCallableAttribute(string? alias = null)
        {
            Alias = alias;
        }
    }
}
