using DIL.Interfaces;
using System.Text.RegularExpressions;

namespace DIL.Middlewares
{
    /// <summary>
    /// Middleware to trim excessive whitespace from the input.
    /// </summary>
    public class TrimWhitespaceMiddleware : IMiddleware
    {
        public string Process(string input)
        {
            // Replace multiple spaces with a single space
            return Regex.Replace(input, @"\s+", " ").Trim();
        }
    }
}