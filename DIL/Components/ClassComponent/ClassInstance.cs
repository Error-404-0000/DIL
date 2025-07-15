using System;
using System.Collections.Generic;

namespace DIL.Components.ClassComponents
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

        //public object? GetProperty(string propertyName)
        //{
        //    var properies = propertyName.Split("->")??[""];
        //    if (!_instanceProperties.TryGetValue(properies[0], out var value))
        //        throw new Exception($"Property '{propertyName}' does not exist in instance of class '{_classDefinition.Name}'.");
        //    var inst= value;
        //    foreach (var prop in properies.Skip(1))
        //    {
        //        if(inst is Dictionary<string, object?> dic)
        //        {
        //            if (!dic.TryGetValue(prop, out var result))
        //                throw new Exception($"Property '{propertyName}' does not exist in instance of class '{_classDefinition.Name}'.");
        //             inst = result;
        //        }
        //        else if (inst is ClassDefinition classDefinition)
        //        {
        //            if (!classDefinition.Fields.TryGetValue(prop, out var result))
        //                throw new Exception($"Property '{propertyName}' does not exist in instance of class '{_classDefinition.Name}'.");
        //            inst = result;
        //        }
        //        else
        //        {
        //            inst = prop.GetType().GetProperty(propertyName)??throw new NotImplementedException($"object '{prop}' is not implemented in {propertyName[0]}");
        //        }
        //    }
        //    return inst;
        //}
        public object? GetProperty(string propertyName)
        {
            var properties = propertyName.Split("->") ?? Array.Empty<string>();

            object? inst;

            // Handle cases where the first property includes indexing, e.g., "Key[1]"
            int bracketIndex = properties[0].IndexOf('[');
            if (bracketIndex != -1)
            {
                // Extract base property name and indexes
                string basePropName = properties[0].Substring(0, bracketIndex).Trim();
                var indexes = ExtractIndexes(properties[0], bracketIndex);

                // Retrieve the base property from _instanceProperties
                if (!_instanceProperties.TryGetValue(basePropName, out var baseValue))
                    throw new Exception($"Property '{basePropName}' does not exist in instance of class '{_classDefinition.Name}'.");

                // Apply indexes to the base property
                inst = ApplyIndexes(baseValue, indexes, basePropName);
            }
            else
            {
                properties[0] = properties[0].Trim();
                // Standard retrieval for non-indexed properties
                if (!_instanceProperties.TryGetValue(properties[0], out var value))
                    throw new Exception($"Property '{properties[0]}' does not exist in instance of class '{_classDefinition.Name}'.");

                inst = value;
            }

            // Process remaining properties in the chain
            foreach (var prop in properties.Skip(1))
            {
                int innerBracketIndex = prop.IndexOf('[');
                if (innerBracketIndex != -1)
                {
                    // Handle property with indexing (e.g., Keys[2] or Keys[2][1])
                    string basePropName = prop.Substring(0, innerBracketIndex).Trim();
                    var indexes = ExtractIndexes(prop, innerBracketIndex);

                    // Retrieve the base property first
                    inst = GetBaseProperty(inst, basePropName, propertyName);

                    // Apply indexes on the retrieved base property
                    inst = ApplyIndexes(inst, indexes, basePropName);
                }
                else
                {
                    // Standard property retrieval
                    inst = GetBaseProperty(inst, prop, propertyName);
                }
            }

            return inst;
        }

        // Helper to extract indexes from the property string (e.g., Keys[2][1] -> [2, 1])
        private List<int> ExtractIndexes(string prop, int startIndex)
        {
            var indexes = new List<int>();
            int i = startIndex;

            while (i < prop.Length)
            {
                int openBracket = prop.IndexOf('[', i);
                if (openBracket == -1)
                    break;

                int closeBracket = prop.IndexOf(']', openBracket);
                if (closeBracket == -1)
                    throw new Exception($"Malformed index syntax in property '{prop}'.");

                string indexStr = prop.Substring(openBracket + 1, closeBracket - openBracket - 1);
                if (!int.TryParse(indexStr, out int index))
                    throw new Exception($"Invalid index '{indexStr}' in property '{prop}'.");

                indexes.Add(index);
                i = closeBracket + 1;
            }

            return indexes;
        }

        // Helper to get the base property value
        private object? GetBaseProperty(object? inst, string propName, string fullPropertyName)
        {
            if (inst is Dictionary<string, object?> dic)
            {
                if (!dic.TryGetValue(propName, out var result))
                    throw new Exception($"Property '{propName}' does not exist in instance of class '{_classDefinition.Name}'.");
                return result;
            }
            else if (inst is ClassDefinition classDefinition)
            {
                if (!classDefinition.Fields.TryGetValue(propName, out var result))
                    throw new Exception($"Property '{propName}' does not exist in instance of class '{_classDefinition.Name}'.");
                return result;
            }
            else
            {
                // Handle standard objects via reflection
                var propertyInfo = inst.GetType().GetProperty(propName);
                
                if (propertyInfo == null)
                    throw new Exception($"Property '{propName}' does not exist in instance of class '{_classDefinition.Name}'.");
                return propertyInfo.GetValue(inst);
            }
        }

        // Helper to apply indexes on a collection
        private object? ApplyIndexes(object? inst, List<int> indexes, string propName)
        {
           
            foreach (var index in indexes)
            {
                if (inst is System.Collections.IList list)
                {
                    if (index < 0 || index >= list.Count)
                        throw new IndexOutOfRangeException($"Index '{index}' out of range for property '{propName}'.");
                    inst = list[index];
                }
                else
                {
                    throw new Exception($"Property '{propName}' is not a list/array but was accessed with indexes.");
                }
            }

            return inst;
        }

        public void SetProperty(string propertyName, object? value)
        {
            var properties = propertyName.Split("->") ?? Array.Empty<string>();
            propertyName = propertyName.Trim();
            // If there's no property name, just return
            if (properties.Length == 0)
                return;

            object? inst = null;
            object? parentInst = null;
            string? currentPropName = null;
            List<int>? currentIndexes = null;

            // Handle the first property, which might have indexing
            int bracketIndex = properties[0].IndexOf('[');
            if (bracketIndex != -1)
            {
                // We have something like "PropName[0]"
                string basePropName = properties[0].Substring(0, bracketIndex).TrimEnd();
                var indexes = ExtractIndexes(properties[0], bracketIndex);

                // Retrieve the base property from _instanceProperties
                if (!_instanceProperties.TryGetValue(basePropName, out var baseValue))
                    throw new Exception($"Property '{basePropName}' does not exist in instance of class '{_classDefinition.Name}'.");

                // After getting the base property, we keep track of it and indexes
                parentInst = _instanceProperties; // The parent is the root dictionary
                inst = baseValue;
                currentPropName = basePropName;
                currentIndexes = indexes;
            }
            else
            {
                // Non-indexed top-level property
                properties[0] = properties[0].Trim();
                if (!_instanceProperties.TryGetValue(properties[0], out var baseValue))
                    throw new Exception($"Property '{properties[0]}' does not exist in instance of class '{_classDefinition.Name}'.");

                parentInst = _instanceProperties; // The parent is the root dictionary
                inst = baseValue;
                currentPropName = properties[0];
                currentIndexes = null;
            }

            // Traverse the property chain
            for (int i = 1; i < properties.Length; i++)
            {
                var prop = properties[i].Trim() ;
                int innerBracketIndex = prop.IndexOf('[');

                // Move down the chain, so the current inst becomes the parent
                parentInst = inst;

                if (innerBracketIndex != -1)
                {
                    string basePropName = prop.Substring(0, innerBracketIndex).Trim() ;
                    var indexes = ExtractIndexes(prop, innerBracketIndex);

                    // Retrieve the next property
                    inst = GetBaseProperty(inst, basePropName, propertyName);

                    currentPropName = basePropName;
                    currentIndexes = indexes;
                }
                else
                {
                    // Non-indexed property in the chain
                    inst = GetBaseProperty(inst, prop, propertyName);

                    currentPropName = prop;
                    currentIndexes = null;
                }
            }

            // Now 'inst' should be the final property or the final collection in which we must set the value.
            // 'parentInst' should be the object that holds the property we want to set.

            // We must distinguish between setting a simple property and setting an indexed element in a collection.
            if (currentIndexes != null && currentIndexes.Count > 0)
            {
                // We are setting an indexed value in a list/array
                // Retrieve the parent property that holds the list
                object? targetList = null;

                // If parentInst is the root dictionary:
                if (parentInst is Dictionary<string, object?> parentDict && currentPropName != null)
                {
                    if (!parentDict.TryGetValue(currentPropName, out targetList))
                        throw new Exception($"Property '{currentPropName}' does not exist.");
                }
                else if (parentInst is ClassDefinition classDef && currentPropName != null)
                {
                    if (!classDef.Fields.TryGetValue(currentPropName, out targetList))
                        throw new Exception($"Property '{currentPropName}' does not exist in instance of class '{_classDefinition.Name}'.");
                }
                else
                {
                    // Parent is a normal object, use reflection
                    var propertyInfo = parentInst?.GetType().GetProperty(currentPropName!);
                    if (propertyInfo == null)
                        throw new Exception($"Property '{currentPropName}' does not exist in instance of class '{_classDefinition.Name}'.");
                    targetList = propertyInfo.GetValue(parentInst);
                }

                // Traverse indexes until the last one, and set the value at that index
                if (targetList is System.Collections.IList list)
                {
                    // If there's more than one index, navigate through them
                    for (int i = 0; i < currentIndexes.Count - 1; i++)
                    {
                        int idx = currentIndexes[i];
                        if (idx < 0 || idx >= list.Count)
                            throw new IndexOutOfRangeException($"Index '{idx}' out of range for property '{currentPropName}'.");
                        list = list[idx] as System.Collections.IList
                            ?? throw new Exception($"Nested indexing found on a non-list element for '{currentPropName}'.");
                    }

                    int finalIndex = currentIndexes[^1];
                    if (finalIndex < 0 || finalIndex >= list.Count)
                        throw new IndexOutOfRangeException($"Index '{finalIndex}' out of range for property '{currentPropName}'.");

                    list[finalIndex] = value;
                }
                else
                {
                    throw new Exception($"Property '{currentPropName}' is not a list/array but was accessed with indexes.");
                }

                // Update the parent property with the modified list if needed
                if (parentInst is Dictionary<string, object?> dictParent && currentPropName != null)
                {
                    dictParent[currentPropName] = targetList;
                }
                else if (parentInst is ClassDefinition classParent && currentPropName != null)
                {
                    classParent.Fields[currentPropName] = targetList;
                }
                else if (parentInst != null && currentPropName != null)
                {
                    var propertyInfo = parentInst.GetType().GetProperty(currentPropName);
                    if (propertyInfo != null && propertyInfo.CanWrite)
                        propertyInfo.SetValue(parentInst, targetList);
                }
            }
            else
            {
                // Setting a non-indexed property
                if (parentInst is Dictionary<string, object?> dic && currentPropName != null)
                {
                    if (!dic.ContainsKey(currentPropName))
                        throw new Exception($"Property '{currentPropName}' does not exist in instance of class '{_classDefinition.Name}'.");
                    dic[currentPropName] = value;
                }
                else if (parentInst is ClassDefinition classDef && currentPropName != null)
                {
                    if (!classDef.Fields.ContainsKey(currentPropName))
                        throw new Exception($"Property '{currentPropName}' does not exist in instance of class '{_classDefinition.Name}'.");
                    classDef.Fields[currentPropName] = value;
                }
                else if (parentInst != null && currentPropName != null)
                {
                    var propertyInfo = parentInst.GetType().GetProperty(currentPropName);
                    if (propertyInfo == null || !propertyInfo.CanWrite)
                        throw new Exception($"Property '{currentPropName}' does not exist or cannot be written in instance of class '{_classDefinition.Name}'.");
                    propertyInfo.SetValue(parentInst, value);
                }
                else
                {
                    throw new Exception($"Unable to set property '{propertyName}'.");
                }
            }
        }


        public object? CallMethod(string methodName, params object[] args)
        {
            if (!_classDefinition.Methods.TryGetValue(methodName, out var method))
                throw new Exception($"Method '{methodName}' does not exist in class '{_classDefinition.Name}'.");
            return method(args);
        }
    }
}
