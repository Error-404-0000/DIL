using DIL.Attributes;
using DIL.Components.ValueComponent;
using DIL.Components.ValueComponent.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DIL.Components
{

    /// <summary>
    /// Handles the FOREACH loop structure in the DSL.
    /// 
    /// Supports two forms:
    /// 1. Inline form using <c>=&gt;</c>:
    /// <code>
    /// FOREACH item IN items => PRINTF item;
    /// </code>
    /// 
    /// 2. Multiline block using <c>DO ;</c> and ending with <c>ENDFOREACH</c>:
    /// <code>
    /// FOREACH item IN items DO ;
    ///     PRINT item;
    ///     PRINTF value;
    /// ENDFOREACH;
    /// </code>
    /// 
    /// Key-value iteration over maps is also supported:
    /// <code>
    /// FOREACH key, val IN myMap DO ;
    ///     PRINT key;
    ///     PRINT ": ";
    ///     PRINTF val;
    /// ENDFOREACH;
    /// </code>
    /// </summary>
    [Obsolete("DO NOT USE")]
    [AutoInterpret(Alias: "FOREACH")]
    public class ForEachComponent
    {
        private class LoopContext
        {
            public string? KeyVar;
            public string ValueVar;
            public List<object> Keys = new();
            public List<object> Values = new();
            public int Index;
            public int StartLine;
            public bool KeyCreated;
            public bool ValueCreated;
        }

        private static Stack<LoopContext> loopStack = new();

        [RegexUse(@"FOREACH\s+([a-zA-Z_]\w*)(?:\s*,\s*([a-zA-Z_]\w*))?\s+IN\s+(.+?)\s*(DO|=>)\s*(.*)?$")]
        public void ForEachStart(
            [FromRegexIndex(1)] string valueVar,
            [FromRegexIndex(2)] string keyVar,
            [FromRegexIndex(3)] string collectionExpr,
            [FromRegexIndex(4)] string loopType,
            [FromRegexIndex(5)] string inlineStatements,
            [CorePassCurrentLine_IndexAttribute] int line,
            [CorePassLinesAttribute] string[] lines,
            [CoreUpdateLineBy] out int jump_by)
        {
           
            var result = LetParser.Parse(collectionExpr);

            List<object> keys = new();
            List<dynamic> values = new();

            if (result is IDictionary<string, object> dict)
            {
                foreach (var kv in dict)
                {
                    keys.Add(kv.Key);
                    values.Add(kv.Value);
                }
            }
            else if (result is System.Collections.IEnumerable list)
            {
                values = list.Cast<dynamic>().ToList();
            }
            else
            {
                throw new Exception("FOREACH expects a list or map.");
            }

            bool keyCreated = false;
            bool valueCreated = false;

            if (!string.IsNullOrEmpty(keyVar))
            {
                if (!LetValueStore.Contains(keyVar))
                {
                    LetValueStore.Set(keyVar, keys.FirstOrDefault() ?? "");
                    keyCreated = true;
                }
                else
                {
                    LetValueStore.NewSet(keyVar, keys.FirstOrDefault() ?? "");
                }
            }

            if (!LetValueStore.Contains(valueVar))
            {
                LetValueStore.Set(valueVar, values.FirstOrDefault() ?? "");
                valueCreated = true;
            }
            else
            {
                LetValueStore.NewSet(valueVar, values.FirstOrDefault() ?? "");
            }

            if (loopType.StartsWith("=>") && !string.IsNullOrWhiteSpace(inlineStatements))
            {
                var stmts = inlineStatements.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var stmt in stmts)
                {
                    var parsed = new InlineEvaluate(stmt).Parse().ToString()!;
                    LetParser.Parse(parsed);
                }
                jump_by = FindJumpTarget(line, lines, "ENDFOREACH");
                return;
            }

            loopStack.Push(new LoopContext
            {
                KeyVar = keyVar,
                ValueVar = valueVar,
                Keys = keys,
                Values = values,
                Index = 0,
                StartLine = line,
                KeyCreated = keyCreated,
                ValueCreated = valueCreated
            });

            jump_by = 0;
        }

        [RegexUse(@"ENDFOREACH")]
        public void ForEachEnd(
            [CorePassCurrentLine_IndexAttribute] int line,
            [CorePassLinesAttribute] string[] lines,
            [CoreUpdateLineBy] out int jump_by)
        {
            if (loopStack.Count == 0)
                throw new Exception("ENDFOREACH without matching FOREACH");

            var context = loopStack.Peek();
            context.Index++;

            if (context.Index >= context.Values.Count)
            {
                loopStack.Pop();
                if (context.KeyCreated && context.KeyVar != null)
                    LetValueStore.PrivateRemove(context.KeyVar);
                if (context.ValueCreated)
                    LetValueStore.PrivateRemove(context.ValueVar);
                jump_by = 0;
                return;
            }

            LetValueStore.NewSet(context.ValueVar, context.Values[context.Index]);
            if (context.KeyVar != null && context.Keys.Count > context.Index)
                LetValueStore.NewSet(context.KeyVar, context.Keys[context.Index]);

            jump_by = context.StartLine - line - 1;
        }

        private int FindJumpTarget(int currentLine, string[] lines, string target)
        {
            for (int i = currentLine + 1; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith(target, StringComparison.OrdinalIgnoreCase))
                    return i - currentLine;
            }
            throw new Exception($"Missing {target} after line {currentLine}.");
        }
    }
}
