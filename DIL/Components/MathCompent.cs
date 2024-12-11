using System;
using DIL.Attributes;
using DIL.BindConvertCompoents;

namespace DIL.Components
{
    /// <summary>
    /// A basic math component that handles simple arithmetic operations.
    /// </summary>
    [AutoInterpret(Alias: "Math")]
    public class MathComponent
    {
        /// <summary>
        /// Adds two numbers.
        /// Example: ADD 10 5
        /// </summary>
        [RegexUse(@"^ADD\s+(\d+)\s+(\d+)$")]
        public int Add([FromRegexIndex(1), ConvertFromString(typeof(ToIntComponents))] int a, [FromRegexIndex(2), ConvertFromString(typeof(ToIntComponents))] int b)
        {
            return a + b;
        }

        /// <summary>
        /// Subtracts the second number from the first.
        /// Example: SUB 10 5
        /// </summary>
        [RegexUse(@"^SUB\s+(\d+)\s+(\d+)$")]
        public int Subtract([FromRegexIndex(1), ConvertFromString(typeof(ToIntComponents))] int a, [FromRegexIndex(2), ConvertFromString(typeof(ToIntComponents)),] int b)
        {
            return a - b;
        }

        /// <summary>
        /// Multiplies two numbers.
        /// Example: MUL 10 5
        /// </summary>
        [RegexUse(@"^MUL\s+(\d+)\s+(\d+)$")]
        public int Multiply([FromRegexIndex(1), ConvertFromString(typeof(ToIntComponents))] int a, [FromRegexIndex(2), ConvertFromString(typeof(ToIntComponents))] int b)
        {
            return a * b;
        }

        /// <summary>
        /// Divides the first number by the second.
        /// Example: DIV 10 5
        /// </summary>
        [RegexUse(@"^DIV\s+(\d+)\s+(\d+)$")]
        public double Divide([FromRegexIndex(1), ConvertFromString(typeof(ToIntComponents))] int a, [FromRegexIndex(2), ConvertFromString(typeof(ToIntComponents))] int b)
        {
            if (b == 0)
            {
                throw new Exception("Math Error: Division by zero is not allowed.");
            }

            return (double)a / b;
        }
    }
}
