using DIL.Attributes;
using DIL.Components.ValueComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIL.Components
{
    [AutoInterpret(Alias:"GET")]
    public class GetComponent
    {
        /// <summary>
        /// Retrieves a stored variable or a property/index of it dynamically.
        /// If the variable does not exist, returns null.
        /// get value
        /// get value->tree->array1[2]
        /// </summary>
        [RegexUse(@"^(?:GET|get|Get)\s+(.+)$")]
        public object? GetVariable([FromRegexIndex(1)] string query)
        {
            var e =  LetDynamicHandler.HandleGet(query);
            return e;
        }
        public void NewSetByRef(string name,ref object newvalue)
        {
           LetValueStore.NewSet(name, newvalue);

        }
        private string ISStr(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                return value.Substring(1, value.Length - 2); // Remove surrounding quotes
            }
            return value;
        }

        private object EvaluateQuery(string query)
        {
            // Handle string literal
            if (query.StartsWith("\"") && query.EndsWith("\""))
            {
                return ISStr(query); // Already a string
            }

            // Handle numeric values
            if (double.TryParse(query, out double numericValue))
            {
                return numericValue;
            }

            // Attempt to resolve from LetDynamicHandler
            try
            {
                var result = LetDynamicHandler.HandleGet(query);
                return result;
            }

            catch
            {
                // Fall back to parsing expression
                return LetParser.Parse(query);
            }
        }

        [RegexUse(@"^(?:PRINTF|Printf|printf)\s+(.+)$")]
        public void PrintFValue([FromRegexIndex(1)] string query)
        {
            var value = EvaluateQuery(query);
            Console.WriteLine(value);
        }

        [RegexUse(@"^(?:PRINT|Print|print)\s+(.+)$")]
        public void PrintValue([FromRegexIndex(1)] string query)
        {
            var value = EvaluateQuery(query);
            Console.Write(value);
        }

        public void NewSet(string name, object newvalue)
        {
            LetValueStore.NewSet(name, newvalue);

        }
    }
}
