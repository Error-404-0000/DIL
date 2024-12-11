namespace DIL.Interfaces
{
    /// <summary>
    /// Interface for classes that handle regex-based calls.
    /// </summary>
    public interface IRegexCallable
    {
        /// <summary>
        /// Matches a method to a regex pattern and executes it.
        /// </summary>
        /// <param name="regexPattern">The regex pattern to match.</param>
        /// <param name="parameters">The parameters to pass to the method.</param>
        /// <returns>The result of the execution.</returns>
        object? CallByRegex(string regexPattern, params object[] parameters);
    }
}
