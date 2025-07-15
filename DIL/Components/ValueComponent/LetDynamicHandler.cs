using DIL.Components.ClassComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
                if (value is IEnumerable list_ )
                {
                    var list = list_.Cast<object>().ToArray();
                    int index = -1;
                    if (Regex.Match(parts[i], @"^\d+$").Success && (int.TryParse(parts[i], out index)));
                    else
                    {
                        var GetVarValue = LetDynamicHandler.HandleGet(parts[i]).ToString();
                        
                        index = (int)LetParser.Parse(GetVarValue, "int");
                        
                    }
                    if (index < 0 || index >= list.Length)
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
                else if(value is ClassInstance c)
                {
                    //User->Type,User->... for ClassInstanceS
                    value = c.GetProperty(parts[i]);
                }
               
                else
                {
                    throw new Exception($"Invalid query part '{parts[i]}' for variable '{baseKey}'.");
                }
            }
            return value;
        }
    }
}
