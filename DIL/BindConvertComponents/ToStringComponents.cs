using DIL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIL.BindConvertComponents
{
    public class ToStringComponents : IBindConvert
    {
        public dynamic? Convert(dynamic? value)
        {
            return System.Convert.ToString(value);
        }
    }
}
