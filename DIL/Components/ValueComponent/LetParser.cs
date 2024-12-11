using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DIL.Components.ValueComponent
{
    public static class LetParser
    {
        /// <summary>
        /// Parses a value expression into a dynamically inferred type.
        /// </summary>
        public static object Parse(string valueExpression, string? type = null)
        {
            object parsedValue;

            if (type != null)
            {
                // If a type is explicitly provided, parse and validate against the type
                parsedValue = ParseWithType(valueExpression, type);
            }
            else
            {
                // Infer the type dynamically if no type is provided
                parsedValue = InferAndParse(valueExpression);
            }

            return parsedValue;
        }

        /// <summary>
        /// Parses the value expression with the specified type.
        /// </summary>
        private static object ParseWithType(string valueExpression, string type)
        {
            var parsedValue = InferAndParse(valueExpression,type);

            // Validate the parsed value against the provided type
            if (!LetValidator.Validate(parsedValue, type))
            {
                throw new Exception($"Value '{valueExpression}' does not match the expected type '{type}'.");
            }

            return LetValidator.Cast(parsedValue, type);
        }

        /// <summary>
        /// Infers the type of the value expression and parses it accordingly.
        /// </summary>
        private static object InferAndParse(string valueExpression, string? type = null)
        {
            if (!string.IsNullOrEmpty(type))
            {
                return type.ToLower() switch
                {
                    "array" => ParseArray(valueExpression),
                    "map" => ParseMap(valueExpression),
                    "int" when int.TryParse(valueExpression, out var intValue) => intValue,
                    "double" when double.TryParse(valueExpression, out var doubleValue) => doubleValue,
                    "string" => valueExpression.Trim('\"'),
                    "binary" when Regex.IsMatch(valueExpression, @"^[01]+$") => ParseBinary(valueExpression),
                    "object" => valueExpression,
                    _ => throw new ArgumentException($"Type '{type}' can't be cast.")
                };
            }
            
            // Infer type if 'type' is null
            if (valueExpression.StartsWith("[") && valueExpression.EndsWith("]"))
            {
                return ParseArray(valueExpression); // Infer as array
            }
            else if (valueExpression.StartsWith("{") && valueExpression.EndsWith("}"))
            {
                return ParseMap(valueExpression); // Infer as map
            }
            else if (int.TryParse(valueExpression, out var intValue))
            {
                return intValue; // Infer as integer
            }
            else if (double.TryParse(valueExpression, out var doubleValue))
            {
                return doubleValue; // Infer as double
            }
            else if (valueExpression.StartsWith("\"") && valueExpression.EndsWith("\""))
            {
                return valueExpression.Trim('\"'); // Infer as string
            }
            else if (Regex.IsMatch(valueExpression, @"^[01]+$"))
            {
                return ParseBinary(valueExpression); // Infer as binary
            }
            else
            {
                throw new ArgumentException($"Cannot determine type for value: '{valueExpression}'.");
            }
        }


        private static string GetFrom(int start, int end, string value)
        {
            if (start < 0 || end > value.Length || start >= end)
                throw new ArgumentOutOfRangeException("Invalid range specified for GetFrom.");

            return value.Substring(start, end - start);
        }

        /// <summary>
        /// Parses a nested array expression into a multidimensional array.
        /// </summary>
        private static IList ParseArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("Empty array cannot be parsed.");

            if(value.StartsWith("[") && value.EndsWith("]"))
            {
                value = GetFrom(1,value.Length-1,value);
            }
            

            var elements = new List<object>();
            int start = 0;
            int bracketDepth = 0;

            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];

                if (current == '[')
                {
                    bracketDepth++;
                }
                else if (current == ']')
                {
                    bracketDepth--;
                }

                // If at the top level and encountering a comma, split elements
                if (bracketDepth == 0 && current == ',')
                {
                    var element = value.Substring(start, i - start).Trim();
                    elements.Add(element.StartsWith("[") ? ParseArray(element) : InferAndParse(element));
                    start = i + 1;
                }
            }

            // Add the last element
            var lastElement = value.Substring(start).Trim();
            elements.Add(lastElement.StartsWith("[") ? ParseArray(lastElement) : InferAndParse(lastElement));

            return elements;
        }

        /// <summary>
        /// Parses a map (dictionary) expression into a dictionary object.
        /// </summary>
        private static IDictionary<string, object?> ParseMap(string value)
        {
            var map = new Dictionary<string, object?>();
            var matches = Regex.Matches(value, @"(\w+)\s*:\s*((""[^""]*"")|(\{(?:[^{}]*|(?<Open>\{)|(?<-Open>\}))*\}(?(Open)(?!)))|(\[(?:[^\[\]]*|(?<Open>\[)|(?<-Open>\]))*\](?(Open)(?!)))|([^,{}]+))");

            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var val = InferAndParse(match.Groups[2].Value); // Parse each map value dynamically
                map[key] = val;
            }

            return map;
        }

        /// <summary>
        /// Parses a binary value into an integer or byte array.
        /// </summary>
        private static object ParseBinary(string value)
        {
            if (value.Length % 8 == 0)
            {
                // Infer as byte array if length is a multiple of 8
                var bytes = new List<byte>();
                for (int i = 0; i < value.Length; i += 8)
                {
                    var binarySegment = value.Substring(i, 8);
                    bytes.Add(Convert.ToByte(binarySegment, 2));
                }
                return bytes.ToArray();
            }

            // Infer as integer if not divisible by 8
            return Convert.ToInt32(value, 2);
        }
    }
}
