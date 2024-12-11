using System;
using System.Collections.Generic;

namespace DIL.Interfaces
{
    /// <summary>
    /// Base interface for all interpreter components.
    /// </summary>
    public interface IInterpreterComponent
    {
        /// <summary>
        /// Executes the component dynamically.
        /// </summary>
        /// <param name="parameters">Parameters to pass to the component.</param>
        /// <returns>The result of the execution.</returns>
        object? Execute(params object[] parameters);
    }
}
