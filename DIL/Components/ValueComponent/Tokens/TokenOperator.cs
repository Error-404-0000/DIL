using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIL.Components.ValueComponent.Tokens
{
    public enum TokenOperator
    {
        None, // For non-operator tokens
        Add, // +
        Subtract, // -
        Multiply, // *
        Divide, // /
        Modulus, // %
        BitwiseAnd, // &
        BitwiseOr, // |
        LogicalAnd, // &&
        LogicalOr, // ||
        GreaterThan, // >
        LessThan, // <
        Equal, // ==
        NotEqual // !=
        // Add more operators as needed
    }


}
