using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DIL.Middlewares;
using DIL.Attributes;
using DIL.Interfaces;

namespace DIL.Core
{
    /// <summary>
    /// The core dynamic interpreter that processes input, executes commands, and provides detailed error reporting.
    /// </summary>
    public class InterpreterCore
    {
        private readonly List<IMiddleware> _middlewares = new();
        private readonly Dictionary<string, Type> _registeredComponents = new();
        private int excute_line = 0;

        /// <summary>
        /// Registers a middleware to the interpreter pipeline.
        /// </summary>
        /// <param name="middleware">The middleware to add.</param>
        public void AddMiddleware(IMiddleware middleware)
        {
            if (_middlewares.Contains(middleware))
                return;
            _middlewares.Add(middleware);
        }

        /// <summary>
        /// Registers a class or component with the interpreter for dynamic execution.
        /// </summary>
        /// <param name="componentType">The type of the component to register.</param>
        public void RegisterComponent(Type componentType)
        {
            if (componentType == null)
                throw new ArgumentNullException(nameof(componentType));

            var alias = componentType.GetCustomAttribute<AutoInterpretAttribute>()?.Alias ?? componentType.Name;
            _registeredComponents[alias] = componentType;
        }

        public InterpreterCore()
        {
            AddMiddleware(new RemoveCommentsMiddleware());
            AddMiddleware(new FlattenMultilineMiddleware());
            AddMiddleware(new TrimWhitespaceMiddleware());
        }

        private List<string> lines = new List<string>();

        /// <summary>
        /// Processes and executes the given input dynamically.
        /// </summary>
        /// <param name="input">The raw input code to execute.</param>
        /// <returns>The result of the execution.</returns>
        public object? Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // Step 1: Run input through the middleware pipeline
            string processedInput = RunMiddleware(input);

            // We use a single-line regex pattern that splits on either ';' or 'end' (in various cases),
            // but not within strings. The pattern uses lookbehind to split lines whenever we encounter
            // a start-of-line or one of our delimiters (e.g., ';', 'end', 'End', 'END'), outside of quotes.
            // We do not consider ';' or 'end' inside quotes as delimiters.
            //
            // The pattern explanation:
            // (?<=^|;|end|End|END) means we look behind for start, a semicolon, or 'end' variants.
            // ([^";]*(" [^"]* ")?[^";]*) is a rough pattern that captures a segment potentially containing a quoted string.
            //
            // This ensures we split on ';' or 'end' but not inside strings.
            string pattern = @"(?<=^|;|end|End|END|:)([^"";]*(""(?:[^""]*)""[^"";]*)*)";

            var matches = Regex.Matches(processedInput, pattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                string value = match.Value.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    lines.Add(value);
                }
            }

            // Execute each line one by one
            for (; excute_line < lines.Count; excute_line++)
            {
                try
                {
                    ExecuteLine(lines[excute_line].Trim());
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    DisplayCodeWithError($"{lines[excute_line]} : {ex.InnerException?.Message ?? ex.Message} in line {excute_line + 1}", 1, lines[excute_line].Length - 1);
                    Console.ResetColor();
                    unsafe
                    {
                        Environment.Exit(-1);
                    }
                }
            }

            return null;
        }

        static void DisplayCodeWithError(string code, int startPosition, int errorLength)
        {
            // Print the code
            Console.WriteLine(code);

            // Create underline with ^ characters
            string underline = new string(' ', startPosition) + new string('^', errorLength);

            // Print the underline in red
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(underline);
            Console.ResetColor();
        }

        /// <summary>
        /// Executes a single line of input dynamically.
        /// Here we find all methods that match and choose the "best" one.
        /// "Best" is defined as the match that has the longest match length or is most specific.
        /// </summary>
        /// <param name="line">The line of input to execute.</param>
        private void ExecuteLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            // Collect all matches from all methods
            List<(MethodInfo method, Type componentType, Match match, string pattern)> potentialMatches = new();

            foreach (var component in _registeredComponents)
            {
                var methods = component.Value.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var regexAttribute = method.GetCustomAttribute<RegexUseAttribute>();
                    var full_i = method.GetCustomAttribute<RegexUseFullInstructionAttribute>();
                    if (regexAttribute != null)
                    {
                        string target = full_i != null ? string.Join("\n", lines.Skip(excute_line)) : line;
                        var match = Regex.Match(target, regexAttribute.Pattern);
                        if (match.Success)
                        {
                            // Store this match for later selection.
                            potentialMatches.Add((method, component.Value, match, regexAttribute.Pattern));
                        }
                    }
                }
            }

            if (!potentialMatches.Any())
            {
                // No full matches found. Let's find the best partial match and where it stops.
                (string pattern, int partialMatchLength) = FindBestPartialMatch(line);

                if (partialMatchLength > 0 && partialMatchLength < line.Length)
                {
                    // Partial match found. Show from the exact character where we fail.
                    // For example, if `class User` matched and the line is `class User<T>:something`,
                    // partialMatchLength would be the length of `class User`. We'll print:
                    // "class User Invalid token "<T>:something""
                    string matchedPart = line.Substring(0, partialMatchLength);
                    string invalidPart = line.Substring(partialMatchLength);

                    throw new Exception($"{matchedPart} Invalid token \"{invalidPart}\"");
                }
                else
                {
                    // No partial match or it's the full line but still invalid.
                    throw new Exception($"Invalid token: \"{line}\"");
                }
            }

            // Pick the best match by longest matched value
            var best = potentialMatches
                .Select(m =>
                {
                    int namedGroups = m.match.Groups.Cast<Group>().Count(g => g.Name != "0" && m.match.Groups[g.Name].Success);
                    int wildcards = Regex.Matches(m.pattern, @"\.\*|\.\+").Count;
                    int patternLength = m.pattern.Length;
                    int matchLength = m.match.Value.Length;

                    int score = namedGroups * 1000 - wildcards * 500 + matchLength * 2 + patternLength;

                    return (score, m);
                })
                .OrderByDescending(x => x.score)
                .First()
                .m;
            ExecuteMethod(best.componentType, best.method, best.match.Value, best.pattern);
        }


        private (string pattern, int matchLength) FindBestPartialMatch(string line)
        {
            int bestLength = 0;
            string bestPattern = string.Empty;

            // Gather all patterns
            List<string> allPatterns = new List<string>();
            foreach (var component in _registeredComponents)
            {
                var methods = component.Value.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var regexAttribute = method.GetCustomAttribute<RegexUseAttribute>();
                    if (regexAttribute != null)
                    {
                        allPatterns.Add(regexAttribute.Pattern);
                    }
                }
            }

            // For each pattern, we try to find the longest prefix of `line` that matches fully.
            // We check progressive substrings from the start.
            foreach (var pattern in allPatterns.Distinct())
            {
                int currentLongest = 0;
                for (int length = 1; length <= line.Length; length++)
                {
                    string substring = line.Substring(0, length);

                    // We want to see if 'substring' fully matches the pattern from start to end.
                    // So we anchor the pattern at the start and end (^ and $).
                    // If it matches, we update currentLongest.
                    if (Regex.IsMatch(substring, "^" + pattern + "$"))
                    {
                        currentLongest = length;
                    }
                    else
                    {
                        // Once it fails for a given length, longer lengths won't match either,
                        // because we're always starting from the beginning of `line`.
                        break;
                    }
                }

                if (currentLongest > bestLength)
                {
                    bestLength = currentLongest;
                    bestPattern = pattern;
                }
            }

            return (bestPattern, bestLength);
        }

        /// <summary>
        /// Executes a method dynamically based on the provided regex match.
        /// </summary>
        internal object? ExecuteMethod(Type componentType, MethodInfo method, string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            if (!match.Success)
                throw new Exception($"Syntax error: \"{input}\" does not match the expected pattern \"{pattern}\"");

            var parameters = method.GetParameters();
            var args = new object?[parameters.Length];
            int excute_line_jump = 0;
            bool is_jump_set = false;

            for (int i = 0; i < parameters.Length; i++)
            {
                var fromRegexIndexAttribute = parameters[i].GetCustomAttribute<FromRegexIndexAttribute>();
                if (fromRegexIndexAttribute != null)
                {
                    if (parameters[i].GetCustomAttribute<ConvertFromStringAttribute>() is ConvertFromStringAttribute cv)
                    {
                        args[i] = cv.IBindConvert.Convert(match.Groups[fromRegexIndexAttribute.Index].Value);
                        continue;
                    }

                    args[i] = match.Groups[fromRegexIndexAttribute.Index].Value;
                    continue;
                }
                if (parameters[i].GetCustomAttribute<CoreUpdateLineByAttribute>() is not null && parameters[i].IsOut && !is_jump_set)
                {
                    is_jump_set = true;
                    excute_line_jump = i;
                    continue;
                }
                if (parameters[i].GetCustomAttribute<CorePassCurrentLine_IndexAttribute>() is not null && parameters[i].ParameterType == typeof(int))
                {
                    args[i] = excute_line;
                    continue;
                }
                if (parameters[i].GetCustomAttribute<CorePassLinesAttribute>() is not null && parameters[i].ParameterType == typeof(string[]))
                {
                    args[i] = lines.ToArray();
                    continue;
                }
                else
                {
                    args[i] = null; // Default value for optional parameters.
                }
            }

            var instance = Activator.CreateInstance(componentType);
            var result = method.Invoke(instance, args);
            if(args.Count() is not 0)
                excute_line += args[excute_line_jump]is int v?v:0;
            return result;
        }

        /// <summary>
        /// Runs the input through the middleware pipeline.
        /// </summary>
        /// <param name="input">The raw input code.</param>
        /// <returns>The processed input.</returns>
        private string RunMiddleware(string input)
        {
            string result = input;
            foreach (var middleware in _middlewares)
            {
                result = middleware.Process(result);
            }
            return result;
        }
    }
}
