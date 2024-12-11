using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DIL.Middlewares;
using DIL.Attributes;
using DIL.Interfaces;
using DIL.Components.ClassComponent;

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

            // Step 2: Execute each line of the processed input
            //string pattern = @"(?<=^|;|\n|end|End|END)([^"";]*(""[^""]*"")?[^"";]*)";

            //var matches = Regex.Matches(processedInput, pattern, RegexOptions.IgnoreCase);


            //foreach (Match match in matches)
            //{
            //    string value = match.Value.Trim();
            //    if (!string.IsNullOrEmpty(value))
            //    {
            //        lines.Add(value);
            //    }
            //}
            lines = input.Split([ ";", "\r\n" ], StringSplitOptions.RemoveEmptyEntries).ToList();
            for (; excute_line < lines.Count; excute_line++)
            {
                try
                {
                    ExecuteLine(lines[excute_line].Trim());
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    DisplayCodeWithError($"{lines[excute_line]} : {ex.InnerException?.Message ?? ex.Message} in line {excute_line+1}", 1, lines[excute_line].Length - 1);
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
        /// </summary>
        /// <param name="line">The line of input to execute.</param>
        private void ExecuteLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return; 
            foreach (var component in _registeredComponents)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var methods = component.Value.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var regexAttribute = method.GetCustomAttribute<RegexUseAttribute>();
                 
                    var full_i = method.GetCustomAttribute<RegexUseFullInstructionAttribute>();
                    if (regexAttribute != null && (full_i!=null? Regex.IsMatch(string.Join("\n", lines.Skip(excute_line-1)), regexAttribute.Pattern): Regex.IsMatch(line, regexAttribute.Pattern)))
                    {
                        ExecuteMethod(component.Value, method, line, regexAttribute.Pattern);
                        return;
                    }
                }
            }

            throw new Exception($"Invalid token: \"{line}\"");
        }
        
        /// <summary>
        /// Executes a method dynamically based on regex matching.
        /// </summary>
        internal object? ExecuteMethod(Type componentType, MethodInfo method, string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            if (!match.Success)
                throw new Exception($"Syntax error: \"{input}\" does not match the expected pattern \"{pattern}\"");

            var parameters = method.GetParameters();
            var args = new object?[parameters.Length];
            int excute_line_jump = 0;
            bool is_jump_set=  false;
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
                    args[i] =  excute_line;
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
            var result =  method.Invoke(instance, args);
            excute_line += excute_line_jump;
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
