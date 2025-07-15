using System;
using System.Globalization;
using DIL.Attributes;
using DIL.BindConvertComponents;
using DIL.Components.ClassComponents;
using DIL.Components.ValueComponent;
using DIL.Components.ValueComponent.Tokens;
using Map = System.Collections.Generic.Dictionary<string, object?>;
namespace DIL.Components.ValueComponent
{
    /// <summary>
    /// Handles dynamic LET, GET, and CALL commands for storing, retrieving, and invoking data.
    /// </summary>
    [AutoInterpret(Alias: "LET")]
    public class LetComponent
    {

        /// <summary>
        /// Executes the LET command for dynamically storing variables.
        /// let m = 23 + id;
        /// </summary>
        [RegexUse(@"^let\s+(\w+)\s*=\s*(.+?)(?:\s+as\s+(.+|map))?$")]
        public void LetVariable(
            [FromRegexIndex(1)] string key,
            [FromRegexIndex(2)] string valueExpression,
            [FromRegexIndex(3)] string? type = null
        )
        {
            if (type is null or "" || string.IsNullOrEmpty(type)) type = "object";
            key = key.Trim();

            var value = LetParser.Parse(valueExpression, type);
            LetValueStore.Set(key, value);
        }
        /// <summary>
        /// Executes the LET command for dynamically storing variables.
        ///  m = 23 + id;
        /// </summary>
        [RegexUse(@"^\s*(\w+)\s*=\s*(.+?)(?:\s+as\s+(.+|map))?$")]
        public void SetVariable(
            [FromRegexIndex(1)] string key,
            [FromRegexIndex(2)] string valueExpression,
            [FromRegexIndex(3)] string? type = null
        )
        {
            if (type is null or "" || string.IsNullOrEmpty(type)) type = null;
            key = key.Trim();

            if (type is not "map")
            {
                InlineEvaluate inline = new InlineEvaluate(valueExpression);
                // Parse the value and apply the optional type
                var value = LetParser.Parse(inline.Parse().ToString()!, type);
                LetValueStore.NewSet(key, value);
            }
            else
            {

                var value = LetParser.Parse(valueExpression, type);
                LetValueStore.NewSet(key, value);
            }
        }

        [RegexUse(@"^\s*(.*?)((?:->)(.*))?\s*=\s*(.*?)(?:\s+as\s+(.+|map))?$")]
        public void EditDObject([FromRegexIndex(1)] string key,
            [FromRegexIndex(2)] string Fields,
            [FromRegexIndex(4)] string? newValue,
            [FromRegexIndex(5)] string? type)
        {
            if (Fields.StartsWith("->"))
            {
                Fields = Fields.Substring(2);
            }
            dynamic? typeOf = LetValueStore.Get(key);
           
            if (typeOf is not null &&typeOf is ClassInstance instance)
            {
                var value = LetParser.Parse(newValue ?? "", string.IsNullOrEmpty(type) ? null : type);
                instance.SetProperty(Fields ?? "", value);
                return;
            }
             //if (typeOf is not null&& string.IsNullOrEmpty(type)&&typeOf is Map && type is null)
             //   type = "map";



            SetVariable(key, Fields ?? "", type);
        }
        [RegexUse(@"^count\s+(.*)$")]
        public void Test([FromRegexIndex(1),ConvertFromString(typeof(GetLenghtIBindComponent))]int count)
        {
            Console.WriteLine(count);
        }
    }
}
