namespace DIL.Interfaces
{
    /// <summary>
    /// Interface for all middleware classes.
    /// </summary>
    public interface IMiddleware
    {
        /// <summary>
        /// Processes the input and returns the processed output.
        /// </summary>
        /// <param name="input">The input to process.</param>
        /// <returns>The processed output.</returns>
        string Process(string input);
    }
}
