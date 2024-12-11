using DIL.Attributes;
using DIL.Components.ValueComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIL.Components
{
    [AutoInterpret(Alias:"GET")]
    public class GetComponent
    {
        /// <summary>
        /// Retrieves a stored variable or a property/index of it dynamically.
        /// If the variable does not exist, returns null.
        /// get value
        /// get value->tree->array1[2]
        /// </summary>
        [RegexUse(@"^(?:GET|get|Get)\s+(.+)$")]
        public object? GetVariable([FromRegexIndex(1)] string query)
        {
            return LetDynamicHandler.HandleGet(query);
        }
    }
}
