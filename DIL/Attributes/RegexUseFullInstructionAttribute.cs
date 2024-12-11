using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIL.Attributes
{
    //tells the parser not to use line by line but the full code
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RegexUseFullInstructionAttribute:Attribute;
 
}
