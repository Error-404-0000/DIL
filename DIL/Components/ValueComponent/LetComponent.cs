using System;
using System.Globalization;
using DIL.Attributes;
using DIL.Components.ClassComponent;
using DIL.Components.ValueComponent;
using DIL.Components.ValueComponent.Tokens;

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
        [RegexUse(@"^(?:LET|let|Let)\s+(\w+)\s*=\s*(.+?)(?:\s+as\s+(.+))?$")]
        public void SetVariable(
            [FromRegexIndex(1)] string key,
            [FromRegexIndex(2)] string valueExpression,
            [FromRegexIndex(3)] string? type = null
        )
        {
            if (type is null or "" || string.IsNullOrEmpty(type)) type = "object";
            InlineEvaluate inline = new InlineEvaluate(valueExpression);
            // Parse the value and apply the optional type
            var value = LetParser.Parse(inline.Parse(), type);
            LetValueStore.Set(key, value);
        }
    }
}
