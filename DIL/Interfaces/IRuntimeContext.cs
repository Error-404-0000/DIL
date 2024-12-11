namespace DIL.Interfaces
{
    /// <summary>
    /// Interface for a dynamic interpreter runtime context.
    /// </summary>
    public interface IRuntimeContext
    {
        /// <summary>
        /// Registers a class or method with the runtime context.
        /// </summary>
        /// <param name="component">The component to register.</param>
        void RegisterComponent(IInterpreterComponent component);

        /// <summary>
        /// Executes a command in the runtime context.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The parameters to pass to the command.</param>
        /// <returns>The result of the execution.</returns>
        object? ExecuteCommand(string command, params object[] parameters);
    }
}
