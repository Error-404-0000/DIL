namespace DIL.Interfaces
{
    /// <summary>
    /// Interface for classes that can handle external calls.
    /// </summary>
    public interface IExternalCallable
    {
        /// <summary>
        /// Calls an external method dynamically.
        /// </summary>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="parameters">The parameters to pass to the method.</param>
        /// <returns>The result of the call.</returns>
        object? CallExternal(string methodName, params object[] parameters);
    }
}
