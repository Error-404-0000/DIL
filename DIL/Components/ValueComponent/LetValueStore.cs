using System;
using System.Collections.Generic;
using System.Reflection;

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
            (string Refname, object? FinalValue) x = (null, value);
            
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Variable name cannot be null or empty.");
            else if (_store.ContainsKey(key))
                throw new Exception($"Variable '{key}' isDefined.");
            if (value is string str && !str.StartsWith("\"") && !str.EndsWith("\""))
                x = dymicSet(str);
            _store[key.Trim()] = x.FinalValue;
        }
        /// <summary>
        /// Edit a stored  variable dynamically.
        /// </summary>
        public static void NewSet(string key, object? value)
        {
            (string name, object? FinalValue) x = (null,value);
            if (value is string str && !str.StartsWith("\"") && !str.EndsWith("\""))
                x = dymicSet(str);
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Variable name cannot be empty.");
            else if (!_store.ContainsKey(key))
                throw new Exception($"Variable '{key}' is not Defined.");
            else if (_store[key] is object obl && value is not null&& obl.GetType() != value?.GetType())
                throw new InvalidCastException($"InvalidCastException. Can't cast '{obl.GetType()}' to '{value?.GetType()}'");
            _store[key.Trim()] = x.FinalValue;
        }
        
        public static Type GetType(string key)
        {
            if (!_store.ContainsKey(key.Trim()))
                throw new Exception($"Variable '{key}' does not exist.");
            return _store[key.Trim()]?.GetType() ?? typeof(object);
        }
        /// <summary>
        /// Retrieves a stored variable.
        /// </summary>
        public static object? Get(string key)
        {
            return dymicSet(key).FinalValue;
        }
        /// <summary>
        /// Retrieves a value from a nested structure using a dot-separated string of keys and method calls.
        /// </summary>
        /// <param name="key_steps">A dot-separated string representing the path to the desired member or method in the nested structure.</param>
        /// <returns>The value retrieved from the specified path in the nested structure.</returns>
        /// <exception cref="Exception">Thrown when a specified variable, method, or member does not exist or when a null reference is encountered.</exception>
        private static (string objectRefName,object? FinalValue) dymicSet(string key_steps)
        {
            
            var steps = key_steps.Split('.');
            var name = steps[0].Trim();

            if (!_store.ContainsKey(name))
                throw new Exception($"Variable '{key_steps}' is not defined.");

            dynamic? current = _store[name];

            for (int i = 1; i < steps.Length; i++)
            {
                if (current == null)
                    throw new Exception($"Null reference at '{string.Join('.', steps.Take(i))}'.");

                var type = current.GetType() as Type;
                var rawStep = steps[i].Trim();


                bool isMethod = rawStep.EndsWith("()");

                string memberName = isMethod
                    ? rawStep.Substring(0, rawStep.Length - 2)
                    : rawStep;

                if (isMethod)
                {
                    var method = type.GetMethod(memberName, Type.EmptyTypes);
                    if (method == null)
                        throw new Exception($"Method '{memberName}' not found on type '{type.Name}'.");

                    current = method.Invoke(current, null);
                }
                else
                {
                    var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);
                    if (prop != null)
                    {
                        current = prop.GetValue(current);
                        continue;
                    }

                    var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
                    if (field != null)
                    {
                        current = field.GetValue(current);
                        continue;
                    }

                    throw new Exception($"Member '{memberName}' not found on type '{type.Name}'.");
                }
            }

            return (name, current);
        }
        public static bool Contains(string key)=>_store.ContainsKey(key.Trim());

        internal static void PrivateRemove(string iteratorName)
        {
           if(!Contains(iteratorName))
                { return; }
           _store.Remove(iteratorName);
        }
    }
}
