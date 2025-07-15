using DIL.Components.ClassComponents;
using System;
using System.Collections;

namespace DIL.Components.ValueComponent
{
    public static class LetValidator
    {
        /// <summary>
        /// Validates whether the value matches the desired type dynamically.
        /// </summary>
        public static bool Validate(object? value, string type)
        {
            if (value == null) value = default;
            if (type is null or "" || string.IsNullOrEmpty(type)) type = "object";
            type = type.ToLower();

            return type switch
            {
                "int" => value is int,
                "string" => value is string,
                "array" => value is IList, // General array or list
                "map" => value is IDictionary, // General dictionary
                "object" => true, // Allow any non-null object
                "class"=>value is ClassInstance,
                _ => ValidateComplexType(value, type) // Handle complex or custom types
            };
        }

        /// <summary>
        /// Converts a value into the specified type dynamically.
        /// </summary>
        public static object Cast(object value, string type)
        {
            type = type.ToLower();
          
            return type switch
            {
                "int" => ConvertToInt(value),
                "string" => ConvertToString(value),
                "array" => ConvertToArray(value),
                "map" => ConvertToMap(value),
                "object" => value,
                "class"=>value,
                _ => throw new Exception($"Invalid type casting  '{type}'.")
            };
        }

        /// <summary>
        /// Validates complex or custom types like nested arrays dynamically.
        /// </summary>
        private static bool ValidateComplexType(object value, string type)
        {
            if (type.StartsWith("[") && type.EndsWith("]"))
            {
                // Handle nested arrays dynamically
                return ValidateNestedArray(value, type);
            }

            throw new Exception($"invaild token type: '{type}'.");
        }

        /// <summary>
        /// Validates nested arrays dynamically based on the structure of the value.
        /// </summary>
        private static bool ValidateNestedArray(object value, string type)
        {
            if (!(value is IList list)) return false;

            // Strip one level of brackets (e.g., "[][]" -> "[]")
            string innerType = type[1..^1];

            foreach (var item in list)
            {
                if (!Validate(item, innerType)) return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a value to an integer.
        /// </summary>
        private static int ConvertToInt(object value)
        {
            if (value is int intValue) return intValue;

            if (int.TryParse(value.ToString(), out int result))
            {
                return result;
            }

            throw new Exception($"Cannot cast value '{value}' to type 'int'.");
        }

        /// <summary>
        /// Converts a value to a string.
        /// </summary>
        private static string ConvertToString(object value)
        {
            return value.ToString() ?? throw new Exception($"Cannot cast value '{value}' to type 'string'.");
        }

        /// <summary>
        /// Converts a value to an array.
        /// </summary>
        private static IList ConvertToArray(object value)
        {
            if (value is IList list) return list;

            throw new Exception($"Cannot cast value '{value}' to type 'array'.");
        }

        /// <summary>
        /// Converts a value to a map (dictionary).
        /// </summary>
        private static IDictionary ConvertToMap(object value)
        {
            if (value is IDictionary dictionary) return dictionary;

            throw new Exception($"Cannot cast value '{value}' to type 'map'.");
        }
    }
}
