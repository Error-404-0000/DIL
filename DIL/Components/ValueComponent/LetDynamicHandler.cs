using System;
using System.Collections;
using System.Collections.Generic;

namespace DIL.Components.ValueComponent
{
    public static class LetDynamicHandler
    {
        /// <summary>
        /// Handles dynamic retrieval queries for arrays, maps, and objects.
        /// </summary>
        public static object? HandleGet(string query)
        {
            var parts = query.Split(new[] { "[", "]", "->" }, StringSplitOptions.RemoveEmptyEntries);
            var baseKey = parts[0].Trim();

            var value = LetValueStore.Get(baseKey)
                ?? throw new Exception($"Variable '{baseKey}' does not exist.");

            for (int i = 1; i < parts.Length; i++)
            {
                if (value is IList list && int.TryParse(parts[i], out var index))
                {
                    if (index < 0 || index >= list.Count)
                        throw new Exception($"Index '{index}' out of range for array '{baseKey}'.");
                    value = list[index];
                }
                else if (value is IDictionary<string, object?> map)
                {
                    if (!map.ContainsKey(parts[i]))
                        throw new Exception($"Key '{parts[i]}' not found in map '{baseKey}'.");
                    value = map[parts[i]];
                }
                else if (value is string str && int.TryParse(parts[i], out var index_str))
                {
                    if (index_str < 0 || index_str >= str.Length)
                        throw new Exception($"Index '{index_str}' out of range for array '{baseKey}'.");
                    value = str[index_str];
                }
                else
                {
                    throw new Exception($"Invalid query part '{parts[i]}' for variable '{baseKey}'.");
                }
            }
            Console.WriteLine(value);
            return value;
        }
    }
}
