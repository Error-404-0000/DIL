using DIL.Components.ValueComponent.Tokens;
using DIL.Components.ValueComponent;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;

namespace DIL.Components.ClassComponents
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
        public enum ParameterType
        {
            DirectValue,
            Class,
            Object,
            Arrary,
            Map,
            String,
            Int
        } 
        public record Parameter(string ParameterName,ParameterType ParameterType);
        public record FuncDefinition
        {
            public int ParameterCount { get; set; }
            public string Name { get; set; }
            public Parameter[] Parameters { get; set; }
            public string Body {  get; set; }

        }
        public string Name { get; }
        public Dictionary<string, object?> Fields { get; private set; } = new();
        public Dictionary<string, Func<object[], object?>> Methods { get; private set; } = new();
        public ClassDefinition(string name,List<string> bodys)
        {
            Name = name;
            LoadDefaultClassInfo();
            BuildFields(bodys);
        }
        private void LoadDefaultClassInfo()
        {
            Fields.Add("Type",Name);
        }
        public void BuildFields(List<string> str_fields)
        {
            bool forces_set=false;
            foreach (var field in str_fields)
            {
                var FieldMatch = Regex.Match(field, @"^(\w+[\W\d\S]*)\s*\:\s*(.*?)\s*(?:\s+as\s+(.+))?(?:\s*\$(overwrite)\$\s*)?$");
                if (FieldMatch.Success)
                {
                    //Type : name $overwrite$
                    if (Fields.ContainsKey(FieldMatch.Groups[1].Value) && FieldMatch.Groups[4].Value!="overwrite")
                    {
                        throw new DuplicateNameException($"DuplicateNameException in {FieldMatch.Value}. use $overwrite$ to overwrite the value.");
                    }
                    if (FieldMatch.Groups[4].Value == "overwrite")
                        forces_set = true;
                    if (Fields.TryGetValue(FieldMatch.Groups[2].Value,out object? value))
                        AddProperty(FieldMatch.Groups[1].Value, value, forces_set);
                    else
                    {
                        
                        // Parse the value and apply the optional type
                        object result = default;
                        if (FieldMatch.Groups[2].Value.StartsWith('[') && FieldMatch.Groups[2].Value.EndsWith(']'))
                        {
                            string arrayContent = FieldMatch.Groups[2].Value;
                            string pattern = @"\[([^,\]]+|""[^""]*"")(?:,|\])?";
                            var matches = Regex.Matches(arrayContent, pattern);

                            // Parse each item in the array
                            var parsedItems = new List<string>();
                            foreach (Match match in matches)
                            {
                                string value_ = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                                InlineEvaluate inline = new InlineEvaluate(value_);
                                parsedItems.Add(inline.Parse().ToString()!);
                            }

                            // Combine the parsed results back into the result variable
                            result = $"[{string.Join(",", parsedItems)}]";
                            result = LetParser.Parse((string)result, "array");
                        }
                        else if (FieldMatch.Groups[3].Value is "class")
                        {
                            //throws error if class IsNotdefined
                            var classDefinition = ClassDefinitionManager.GetClass(FieldMatch.Groups[2].Value);
                            result = classDefinition;
                        }
                        else
                        {
                            if (FieldMatch.Groups[3].Value is not "map")
                            {
                                InlineEvaluate inline = new InlineEvaluate(FieldMatch.Groups[2].Value);
                                result = inline.Parse(true);
                                if (result is not ClassDefinition or ClassInstance)
                                {
                                    result = LetParser.Parse(result.ToString()!, string.IsNullOrEmpty(FieldMatch.Groups[3].Value) ? "object" : FieldMatch.Groups[3].Value);
                                }
                            }
                            else
                            {
                                result = LetParser.Parse(FieldMatch.Groups[2].Value, string.IsNullOrEmpty(FieldMatch.Groups[3].Value) ? "object" : FieldMatch.Groups[3].Value);
                            }
                        }
                        

                        AddProperty(FieldMatch.Groups[1].Value.Trim(),result, forces_set);
                    }
                }
            }
        
        }

        public void AddProperty(string propertyName, object? defaultValue = null,bool force_set = false)
        {
            if (Fields.ContainsKey(propertyName)&&!force_set)
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
