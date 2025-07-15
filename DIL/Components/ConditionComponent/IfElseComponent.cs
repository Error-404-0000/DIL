using System;
using System.Collections.Generic;
using DIL.Attributes;
using DIL.Components.ValueComponent;

namespace DIL.Components
{
    [AutoInterpret(Alias: "IF")]
    public class IfElseComponent
    {
        private static Stack<bool> conditionStack = new();

        /// <summary>
        /// Handles the IF condition.
        /// </summary>
        [RegexUse(@"IF\s+(.+)\s*THEN\s*")]
        public void IfStatement([FromRegexIndex(1)] string condition, [CorePassCurrentLine_IndexAttribute] int line, [CorePassLinesAttribute] string[] lines, [CoreUpdateLineBy] out int jump_by)
        {
            bool conditionResult = EvaluateCondition(condition);
            conditionStack.Push(conditionResult);

            if (!conditionResult)
            {
                // Find the matching ELSE or ENDIF
                jump_by = FindJumpTarget(line, lines, "ELSE", "ENDIF")-1;
            }
            else
            {
                jump_by = 0; // Continue execution
            }
        }

        /// <summary>
        /// Handles ELSE statements.
        /// </summary>
        [RegexUse(@"ELSE")]
        public void ElseStatement([CorePassCurrentLine_IndexAttribute] int line, [CorePassLinesAttribute] string[] lines, [CoreUpdateLineBy] out int jump_by)
        {
            if (conditionStack.Count == 0)
                throw new Exception("ELSE without matching IF.");

            bool lastCondition = conditionStack.Pop();
            if (lastCondition)
            {
                jump_by = FindJumpTarget(line, lines, "ENDIF");
            }
            else
            {
                jump_by = 0; // Execute ELSE block
            }
        }

        /// <summary>
        /// Handles ENDIF.
        /// </summary>
        [RegexUse(@"^ENDIF$")]
        public void EndIfStatement()
        {
            if (conditionStack.Count > 0)
                conditionStack.Pop();
        }

        /// <summary>
        /// Finds where to jump in case of a false condition.
        /// </summary>
        private int FindJumpTarget(int currentLine, string[] lines, params string[] targets)
        {
            for (int i = currentLine + 1; i < lines.Length; i++)
            {
                foreach (var target in targets)
                {
                    if (lines[i].Trim().StartsWith(target, StringComparison.OrdinalIgnoreCase))
                    {
                        return i - currentLine;
                    }
                }
            }
            throw new Exception($"Missing expected statement ({string.Join(" or ", targets)}) after line {currentLine}.");
        }

        /// <summary>
        /// Evaluates a condition dynamically.
        /// </summary>
        private bool EvaluateCondition(string condition)
        {
            object result = null;
            return (result = LetParser.Parse(condition)) is bool boolean?boolean:throw new InvalidDataException($"Unable to Evaluate {result} to boolean") ;
            
        }
    }
}
