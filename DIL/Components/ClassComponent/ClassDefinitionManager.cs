using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

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
        public Dictionary<string, object?> Fields { get; }
        public Dictionary<string, Func<object[], object?>> Methods { get; }

        public ClassDefinition(string name,string body)
        {
            Name = name;
            Fields = BuildFields(body);
        }
        public Dictionary<string, object?> BuildFields(string str_fields)
        {
            Dictionary<string, object?> fields= new Dictionary<string, object?>();
            var matches = Regex.Matches(string.Join(" ", str_fields.Split("\n")), @"(^(\w+[\W\d\S]*)\s*\:\s*(.*)\s*$)*");
            if (matches.Count > 0)
            {
                foreach (Match item in matches)
                {
                    if (fields.ContainsKey(item.Value))
                    {
                        throw new DuplicateNameException($"DuplicateNameException in {item}.");
                    }
                    fields.Add(item.Groups[1].Value, item.Groups[2].Value);
                }
            }
            return fields;
        }

        public void AddProperty(string propertyName, object? defaultValue = null)
        {
            if (Fields.ContainsKey(propertyName))
                throw new Exception($"Property '{propertyName}' is already defined in class '{Name}'.");
            Fields[propertyName] = defaultValue;
        }

        public void AddMethod(string methodName, Func<object[], object?> method)
        {
            if (Methods.ContainsKey(methodName))
                throw new Exception($"Method '{methodName}' is already defined in class '{Name}'.");
            Methods[methodName] = method;
        }
    }
}
