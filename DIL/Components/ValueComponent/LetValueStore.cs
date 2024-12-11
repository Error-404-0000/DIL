using System;
using System.Collections.Generic;

namespace DIL.Components.ValueComponent
{
    /// <summary>
    /// Centralized storage for LET variables.
    /// </summary>
    public static class LetValueStore
    {
        private static readonly Dictionary<string, object?> _store = new();

        /// <summary>
        /// Stores a variable dynamically.
        /// </summary>
        public static void Set(string key, object? value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Variable name cannot be null or empty.");

            _store[key] = value;
        }

        /// <summary>
        /// Retrieves a stored variable.
        /// </summary>
        public static object? Get(string key)
        {
            if (!_store.ContainsKey(key))
                throw new Exception($"Variable '{key}' does not exist.");

            return _store[key];
        }
    }
}
