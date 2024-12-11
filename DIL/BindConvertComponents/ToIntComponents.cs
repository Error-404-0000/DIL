using DIL.Interfaces;

namespace DIL.BindConvertCompoents
{
    public class ToIntComponents : IBindConvert
    {
        public dynamic? Convert(dynamic? value)
        {
            return System.Convert.ToInt32(value);
        }
    }
}
