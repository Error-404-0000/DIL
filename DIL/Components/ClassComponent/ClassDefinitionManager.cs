using System;
using System.Collections.Generic;

namespace DIL.Components.ClassComponent
{
    /// <summary>
    /// Manages class definitions and instances.
    /// </summary>
    public static class ClassDefinitionManager
    {
        private static readonly Dictionary<string, ClassDefinition> ClassRegistry = new();

        /// <summary>
        /// Registers a new class definition.
        /// </summary>
        public static void RegisterClass(string className, ClassDefinition classDefinition)
        {
            if (ClassRegistry.ContainsKey(className))
                throw new Exception($"Class '{className}' is already defined.");
            ClassRegistry[className] = classDefinition;
        }

        /// <summary>
        /// Retrieves a class definition by name.
        /// </summary>
        public static ClassDefinition GetClass(string className)
        {
            if (!ClassRegistry.TryGetValue(className, out var classDefinition))
                throw new Exception($"Class '{className}' is not defined.");
            return classDefinition;
        }

        /// <summary>
        /// Checks if a class is defined.
        /// </summary>
        public static bool IsClassDefined(string className)
        {
            return ClassRegistry.ContainsKey(className);
        }
    }

    /// <summary>
    /// Represents a class definition.
    /// </summary>
    public class ClassDefinition
    {
        public string Name { get; }
        public Dictionary<string, object?> Properties { get; }
        public Dictionary<string, Func<object[], object?>> Methods { get; }

        public ClassDefinition(string name)
        {
            Name = name;
            Properties = new Dictionary<string, object?>();
            Methods = new Dictionary<string, Func<object[], object?>>();
        }

        public void AddProperty(string propertyName, object? defaultValue = null)
        {
            if (Properties.ContainsKey(propertyName))
                throw new Exception($"Property '{propertyName}' is already defined in class '{Name}'.");
            Properties[propertyName] = defaultValue;
        }

        public void AddMethod(string methodName, Func<object[], object?> method)
        {
            if (Methods.ContainsKey(methodName))
                throw new Exception($"Method '{methodName}' is already defined in class '{Name}'.");
            Methods[methodName] = method;
        }
    }
}
