using System;

namespace DIL.Attributes
{

    /// <summary>
    /// Specifies that a property or method is a default entry point for the interpreter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class DefaultEntryPointAttribute : Attribute
    {
    }
}
