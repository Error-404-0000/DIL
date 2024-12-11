using System;
using System.Linq;
using System.Text.RegularExpressions;
using DIL.Attributes;
using DIL.Components.ValueComponent;
using DIL.Core;

namespace DIL.Components.ClassComponent
{
    /// <summary>
    /// Handles dynamic class definitions, instantiation, and member access.
    /// </summary>
    [AutoInterpret(Alias: "CLASS")]
    public class ClassComponent
    {
        /// <summary>
        /// Starts defining a new class. Dynamically calculates lines to skip.
        /// </summary>
        [RegexUse(@"^class:\s*(\w+)\s*$")]
        public void StartClassDefinition(
            [FromRegexIndex(1)] string className,
            [CorePassCurrentLine_IndexAttribute]int line,
            [CorePassLinesAttribute]string[] line_,
            [CoreUpdateLineBy] out int jump_by)
        {
            var classDefinition = new ClassDefinition(className);
            ClassDefinitionManager.RegisterClass(className, classDefinition);
            Console.WriteLine($"Started defining class '{className}'.");

            // Dynamically calculate how many lines to skip (until `class:end`).
            var currentLineIndex = line;
            var lines = line_;
            int skipLines = 0;

            for (int i = currentLineIndex + 1; i < lines.Length; i++)
            {
                if (Regex.IsMatch(lines[i], @"^class:\s*end$"))
                {
                    skipLines = i - currentLineIndex;
                    break;
                }
            }

            if (skipLines == 0)
                throw new Exception($"Missing 'class:end' for class '{className}' starting at line {currentLineIndex}.");

            jump_by = skipLines; // Update the number of lines to skip.
        }

        /// <summary>
        /// Ends the class definition. Confirms that processing stops here.
        /// </summary>
        [RegexUse(@"^class:\s*end$"), RegexUseFullInstruction]
        public void EndClassDefinition([CoreUpdateLineBy] out int jump_by)
        {
            Console.WriteLine("Class definition ended.");
            jump_by = 1; // Skip the `class:end` line.
        }

        /// <summary>
        /// Creates an instance of a defined class.
        /// </summary>
        [RegexUse(@"^new\s+(\w+)\s*=\s*(\w+):new$")]
        public void CreateInstance(
            [FromRegexIndex(1)] string instanceName,
            [FromRegexIndex(2)] string className)
        {
            var classDefinition = ClassDefinitionManager.GetClass(className);
            var instance = new ClassInstance(classDefinition);
            LetValueStore.Set(instanceName, instance);

            Console.WriteLine($"Created instance '{instanceName}' of class '{className}'.");
        }
        private static GetComponent _getComponent = new GetComponent();
        /// <summary>
        /// Retrieves a property of a class instance.
        /// </summary>
        [RegexUse(@"^(?:GET|get|Get)\s+(\w+)->(\w+)$")]
        public object? GetInstanceProperty(
            [FromRegexIndex(1)] string instanceName,
            [FromRegexIndex(2)] string propertyName)
        {
            var instance = LetValueStore.Get(instanceName) as ClassInstance;
            if (instance == null)
            {
                return _getComponent.GetVariable(instanceName+"->"+propertyName);
            }
            return instance.GetProperty(propertyName);
        }

        /// <summary>
        /// Calls a method of a class instance.
        /// </summary>
        [RegexUse(@"^CALL\s+(\w+)->(\w+)\((.*)\)$")]
        public object? CallInstanceMethod(
            [FromRegexIndex(1)] string instanceName,
            [FromRegexIndex(2)] string methodName,
            [FromRegexIndex(3)] string args)
        {
            var instance = LetValueStore.Get(instanceName) as ClassInstance;
            if (instance == null)
                throw new Exception($"Instance '{instanceName}' does not exist or is not a class instance.");

            var parsedArgs = ParseArguments(args);
            return instance.CallMethod(methodName, parsedArgs);
        }

        /// <summary>
        /// Parses arguments for method calls.
        /// </summary>
        private object[] ParseArguments(string args)
        {
            if (string.IsNullOrWhiteSpace(args)) return Array.Empty<object>();
            var argList = args.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var parsedArgs = new object[argList.Length];
            for (int i = 0; i < argList.Length; i++)
            {
                parsedArgs[i] = LetParser.Parse(argList[i].Trim());
            }
            return parsedArgs;
        }
    }
}
