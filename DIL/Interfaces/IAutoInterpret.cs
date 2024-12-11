namespace DIL.Interfaces
{
    /// <summary>
    /// Interface for classes that can be interpreted.
    /// </summary>
    public interface IAutoInterpret
    {
        /// <summary>
        /// Provides a dynamic execution context.
        /// </summary>
        /// <param name="methodName">The name of the method to execute.</param>
        /// <param name="parameters">The parameters to pass to the method.</param>
        /// <returns>The result of the execution.</returns>
        object? InvokeMethod(string methodName, params object[] parameters);

        /// <summary>
        /// Gets or sets a property dynamically.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set (optional).</param>
        /// <returns>The property value if getting; otherwise, null.</returns>
        object? InvokeProperty(string propertyName, object? value = null);
    }
}
