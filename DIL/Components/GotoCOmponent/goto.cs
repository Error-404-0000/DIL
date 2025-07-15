using DIL.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DIL.Components.GotoCOmponent
{
    [AutoInterpret("goto")]
    public class @goto
    {
        [RegexUse(@"^([A-Z a-z ]*\d*)\s*:")]
        public void MarkAsGotoable([FromRegexIndex(1)] string Name,[CorePassCurrentLine_Index] int current_line ,[CorePassLines] string[] lines)
        {
            if (lines.Length == 0) return;
            var li = lines.ToList();
            li.RemoveRange(current_line, 1);
            if (li.Any(x => Regex.Match(x, @$"^{Name}\s*:").Success) )
            {
                throw new DuplicateWaitObjectException("Duplicate Goto. of {Name}.");
            }
        }
        [RegexUse(@"^goto\s+([A-Z a-z ]*\d*)")]
        public void GoToGotoable([FromRegexIndex(1)] string Name, [CorePassLines] string[] lines, [CorePassCurrentLine_Index] int currentline, [CoreUpdateLineBy] out int by)
        {
            if (lines == null || lines.Length == 0)
            {
                throw new ArgumentNullException($"No Gotoable object named {Name}");
            }
            var obj = lines.FirstOrDefault(x => Regex.Match(x, @$"^{Name}\s*:").Success);
            if(obj is null)
                throw new ArgumentNullException($"No Gotoable object named {Name}");
            by = lines.ToList().IndexOf(obj)-currentline;
            

        }
    }
}
