using DIL.Attributes;
using DIL.Components.ValueComponent;
using DIL.Components.ValueComponent.Tokens;

namespace DIL.Components.ForLoop
{
    /// <summary>
    /// Handles the start of a FOR loop with expression-based evaluation.
    /// Supports inline loop body using either <c>DO</c> or <c>=&gt;</c> syntax,
    /// or traditional multi-line blocks ending in <c>ENDFOR</c>.
    /// 
    /// Examples:
    /// <code>
    /// FOR n IN n << 20 DO n = n + 2;
    /// FOR n IN n << 20 => n = n + 2;
    /// FOR n IN n << 20 DO;
    /// FOR n IN n << 20 DO
    ///     PRINT n;
    ///     n = n + 1;
    /// ENDFOR;
    /// </code>
    /// 
    /// Behavior:
    /// - The <paramref name="variableName"/> is automatically initialized to 0 if not defined.
    /// - The <paramref name="valueExpression"/> must evaluate to a boolean.
    /// - If the result is false, the loop is skipped.
    /// - If inline statements are present, they are executed immediately after condition check.
    /// </summary>
    /// <param name="variableName">The loop variable identifier (e.g., 'n').</param>
    /// <param name="valueExpression">The expression to evaluate (must return boolean).</param>
    /// <param name="inlineStatements">Optional inline body code following DO or =>.</param>
    /// <param name="line">The current line index.</param>
    /// <param name="lines">All lines of script source.</param>
    /// <param name="jump_by">Set to number of lines to skip if condition fails.</param>
    ///</summary>




    [AutoInterpret(Alias: "FOR")]
    public class ForLoopComponent
    {
        /// <summary>
        /// Represents the context of a loop, including the iterator name, starting line number, whether it was created
        /// in a loop, and the value expression.
        /// </summary>
        private class LoopContext
        {
            public required string IteratorName;
            public int StartLine;
            public bool CreatedInLoop;
            public required string ValueExpression;
        }


        private static readonly Stack<LoopContext> loopStack = new();
        [RegexUse(@"FOR\s+([a-zA-Z_]\w*)\s+WHEN\s+(.+?)\s*(?:DO|=>)\s*(.*)?$")]
        public void ForStart(
    [FromRegexIndex(1)] string variableName,
    [FromRegexIndex(2)] string valueExpression,
    [FromRegexIndex(3)] string inlineStatements, // may be null or empty
    [CorePassCurrentLine_IndexAttribute] int line,
    [CorePassLinesAttribute] string[] lines,
    [CoreUpdateLineBy] out int jump_by)
        {
            bool createdInLoop = false;

            if (!LetValueStore.Contains(variableName))
            {
                LetValueStore.Set(variableName, 0);
                createdInLoop = true;
            }

            InlineEvaluate inline = new(valueExpression);
            string expr_result = inline.Parse().ToString();
            object result = LetParser.Parse(expr_result);

            if (result is not bool condition)
            {
                throw new Exception($"FOR-IN expression '{valueExpression}' must return a boolean");
            }

            if (!condition)
            {
                jump_by = FindJumpTarget(line, lines, "ENDFOR");
                return;
            }

            // ✅ If there are inline statements, evaluate them now
            if (!string.IsNullOrWhiteSpace(inlineStatements))
            {
                string[] exprs = inlineStatements.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string expr in exprs)
                {
                    InlineEvaluate stmt = new(expr);
                    string parsed = stmt.Parse().ToString()!;
                    object resultEval = LetParser.Parse(parsed);

                    // auto-detect assignment?
                    if (expr.Contains('=') && expr.Contains(variableName))
                    {
                        LetValueStore.NewSet(variableName, resultEval);
                    }
                }
            }

            loopStack.Push(new LoopContext
            {
                IteratorName = variableName,
                StartLine = line,
                CreatedInLoop = createdInLoop,
                ValueExpression = valueExpression
            });

            jump_by = 0;
        }

        [RegexUse(@"ENDFOR")]
        public void ForEnd(
            [CorePassCurrentLine_IndexAttribute] int line,
            [CorePassLinesAttribute] string[] lines,
            [CoreUpdateLineBy] out int jump_by)
        {
            if (loopStack.Count == 0)
            {
                throw new Exception("ENDFOR without matching FOR.");
            }

            LoopContext context = loopStack.Peek();

            // Re-evaluate the original value expression
            InlineEvaluate inline = new(context.ValueExpression);
            string parsedExpr = inline.Parse().ToString()!;
            object result = LetParser.Parse(parsedExpr);

            if (result is not bool condition)
            {
                throw new Exception($"Loop condition '{context.ValueExpression}' must return a boolean");
            }

            if (condition)
            {
                jump_by = context.StartLine - line - 1;
            }
            else
            {
                _ = loopStack.Pop();
                if (context.CreatedInLoop)
                {
                    LetValueStore.PrivateRemove(context.IteratorName);
                }

                jump_by = 0;
            }
        }

        private int FindJumpTarget(int currentLine, string[] lines, string target)
        {
            for (int i = currentLine + 1; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith(target, StringComparison.OrdinalIgnoreCase))
                {
                    return i - currentLine;
                }
            }
            throw new Exception($"Missing {target} after line {currentLine}.");
        }



    }
}
