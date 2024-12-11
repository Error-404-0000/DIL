using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIL.Interfaces
{
    public interface IBindConvert
    {
        dynamic? Convert(dynamic? value);
        public IBindConvert IBind =>this;
    }
}
