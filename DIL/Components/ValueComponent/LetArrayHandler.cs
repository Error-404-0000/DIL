using DIL.Components.ValueComponent;
using System;

namespace DIL.Components.ValueComponent
{
    public static class LetArrayHandler
    {
        /// <summary>
        /// Parses a nested array expression into a multidimensional array.
        /// </summary>
        public static object ParseArray(string value)
        {
            if (!value.StartsWith("[") || !value.EndsWith("]"))
                throw new Exception($"Invalid array syntax: {value}");

            var innerValues = value.Trim('[', ']').Split("],[", StringSplitOptions.RemoveEmptyEntries);
            var array = new object[innerValues.Length][];

            for (int i = 0; i < innerValues.Length; i++)
            {
                var elements = innerValues[i].Trim('[', ']').Split(',');
                array[i] = Array.ConvertAll(elements, e => LetParser.Parse(e.Trim()));
            }

            return array;
        }
    }
}
