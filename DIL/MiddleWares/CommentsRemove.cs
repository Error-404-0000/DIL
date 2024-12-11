using DIL.Interfaces;
using System.Text.RegularExpressions;

namespace DIL.Middlewares
{
    public class RemoveCommentsMiddleware : IMiddleware
    {
        public string Process(string input)
        {
            // Remove single-line comments (// ...)
            string withoutSingleLineComments = Regex.Replace(input, @"//.*?$", "", RegexOptions.Multiline);

            // Remove multi-line comments (/* ... */)
            string withoutMultiLineComments = Regex.Replace(withoutSingleLineComments, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Remove line-ending comments (;;)
            string withoutLineEndingComments = Regex.Replace(withoutMultiLineComments, @";;.*?$", "", RegexOptions.Multiline);

            return withoutLineEndingComments.Trim();
        }
    }
}
