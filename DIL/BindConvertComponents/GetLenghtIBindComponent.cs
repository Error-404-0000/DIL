using DIL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIL.BindConvertComponents
{
    internal class GetLenghtIBindComponent : IBindConvert
    {
        /// <summary>
        /// simple example of impl
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public dynamic? Convert(dynamic? value)
        {
           return value?.ToString().Length??0;
        }
    }
}
