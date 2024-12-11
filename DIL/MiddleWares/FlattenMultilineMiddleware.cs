using DIL.Interfaces;
using System.Text.RegularExpressions;

namespace DIL.Middlewares
{
    /// <summary>
    /// Middleware to flatten multiline statements into a single logical line.
    /// For example:
    /// LET a = 
    /// 30;
    /// becomes:
    /// LET a = 30;
    /// </summary>
    public class FlattenMultilineMiddleware : IMiddleware
    {
        public string Process(string input)
        {
            // Use regex to detect and merge lines that are part of the same statement.
            string pattern = @"(?<statement>[^;]+)\n\s+(?<continuation>[^;]+);";
            string result = Regex.Replace(input, pattern, "${statement} ${continuation};");

            return result;
        }
    }
}
