using System;
using System.Linq;
using System.Text.RegularExpressions;
using DIL.Attributes;
using DIL.Components.ValueComponent;
using DIL.Core;

namespace DIL.Components.ClassComponents
{
    /// <summary>
    /// Handles dynamic class definitions, instantiation, and member access.
    /// </summary>
    [AutoInterpret(Alias: "CLASS")]
    public class ClassComponent
    {
        /// <summary>
        /// Starts defining a new class. Dynamically calculates lines to skip.
        /// 
        //        class Pet :
        //            userid:23;
        //	            userid2:1;
        //      class: end;

        //        let myPet = Pet:new;
        //          let myPet1 = myPet as class;
        //        GET myPet1->userid;

        /// </summary>
        [RegexUse(@"^class\s*(\w+)\s*:\s*(.*)$")]
        public void StartClassDefinition(
            [FromRegexIndex(1)] string className,
            [FromRegexIndex(2)] string _top_regex,
            [CorePassCurrentLine_IndexAttribute] int line,
            [CorePassLinesAttribute] string[] line_,
            [CoreUpdateLineBy] out int jump_by)
        {

            // Dynamically calculate how many lines to skip (until `class:end`).
            var currentLineIndex = line;
            var lines = line_;
            int skipLines = 0;
            List<string> bodys = new();
            bodys.Add(_top_regex);
            for (int i = currentLineIndex + 1; i < lines.Length; i++)
            {

                if (Regex.IsMatch(lines[i], @"^class:\s*end$"))
                {
                    skipLines = i - currentLineIndex;
                    break;
                }
                else
                {
                    bodys.Add(lines[i]);
                }
            }

            if (skipLines == 0)
                throw new Exception($"Missing 'class:end' for class '{className}' starting at line {currentLineIndex}.");
            var classDefinition = new ClassDefinition(className, bodys);
            ClassDefinitionManager.RegisterClass(className, classDefinition);

            jump_by = skipLines;
        }

        /// <summary>
        /// Ends the class definition. Confirms that processing stops here.
        /// class :end;
        /// </summary>
        [RegexUse(@"^class:\s*end$"), RegexUseFullInstruction]
        public void EndClassDefinition([CoreUpdateLineBy] out int jump_by)
        {
            Console.WriteLine("Class definition ended.");
            jump_by = 1; // Skip the `class:end` line.
        }

        /// <summary>
        /// Creates an instance of a defined class.
        /// let class1=Class1:new;
        /// </summary>
        [RegexUse(@"^let\s+(\w+)\s*=\s*(\w+):new$")]
        public void CreateInstance(
            [FromRegexIndex(1)] string instanceName,
            [FromRegexIndex(2)] string className)
        {
            var classDefinition = ClassDefinitionManager.GetClass(className);
            var instance = new ClassInstance(classDefinition);
            LetValueStore.Set(instanceName, instance);


        }
        /// <summary>
        /// Edit an instance of a defined class.
        ///  class1=Class1:new;
        /// </summary>
        [RegexUse(@"^\s*(\w+)\s*=\s*(\w+):new$")]
        public void newInstance(
            [FromRegexIndex(1)] string instanceName,
            [FromRegexIndex(2)] string className)
        {
            var classDefinition = ClassDefinitionManager.GetClass(className);
            var instance = new ClassInstance(classDefinition);
            LetValueStore.NewSet(instanceName, instance);


        }
        private static GetComponent _getComponent = new GetComponent();
        /// <summary>
        /// Retrieves a property of a class instance.
        /// Get Class1->Name
        /// </summary>
        [RegexUse(@"^(?:Get)\s+(\w+)(?:->(\w+.*))?$")]
        public object? GetInstanceProperty(
            [FromRegexIndex(1)] string instanceName,
            [FromRegexIndex(2)] string propertyName)
        {
            try
            {
                var instance = LetValueStore.Get(instanceName) as ClassInstance;
                if (instance == null)
                {
                    return _getComponent.GetVariable(instanceName + "->" + propertyName);
                }

                var result = !string.IsNullOrWhiteSpace(propertyName) ? instance.GetProperty(propertyName) : instance;
                Console.WriteLine(result);

                return result;
            }catch(IndexOutOfRangeException) { throw; }
            catch
            {
                if (propertyName is not "" or null)
                    return _getComponent.GetVariable(instanceName + "->" + propertyName); 
                var result =ClassDefinitionManager.GetClass(/*classname*/instanceName);
                
                return result;
            }
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
