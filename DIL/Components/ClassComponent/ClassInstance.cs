using System;
using System.Collections.Generic;

namespace DIL.Components.ClassComponent
{
    /// <summary>
    /// Manages class instances dynamically.
    /// </summary>
    public class ClassInstance
    {
        private readonly ClassDefinition _classDefinition;
        private readonly Dictionary<string, object?> _instanceProperties;

        public ClassInstance(ClassDefinition classDefinition)
        {
            _classDefinition = classDefinition;
            _instanceProperties = new Dictionary<string, object?>(classDefinition.Fields);
        }

        public object? GetProperty(string propertyName)
        {
            if (!_instanceProperties.TryGetValue(propertyName, out var value))
                throw new Exception($"Property '{propertyName}' does not exist in instance of class '{_classDefinition.Name}'.");
            return value;
        }

        public void SetProperty(string propertyName, object? value)
        {
            if (!_instanceProperties.ContainsKey(propertyName))
                throw new Exception($"Property '{propertyName}' does not exist in instance of class '{_classDefinition.Name}'.");
            _instanceProperties[propertyName] = value;
        }

        public object? CallMethod(string methodName, params object[] args)
        {
            if (!_classDefinition.Methods.TryGetValue(methodName, out var method))
                throw new Exception($"Method '{methodName}' does not exist in class '{_classDefinition.Name}'.");
            return method(args);
        }
    }
}
