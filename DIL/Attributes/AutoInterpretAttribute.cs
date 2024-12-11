namespace DIL.Attributes
{
    /// <summary>
    /// Indicates that a class or method is intended to be interpreted dynamically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class AutoInterpretAttribute : Attribute
    {
        public string? Alias { get; } // Optional alias for easier referencing in the interpreter.

        public AutoInterpretAttribute(string? Alias = null)
        {
            this.Alias = Alias;
        }
    }
}
