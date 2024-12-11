using System;

using DIL.Components;
using DIL.Components.ClassComponent;
using DIL.Components.ValueComponent;
using DIL.Components.ValueComponent.Tokens;
using DIL.Core;
using DIL.Middlewares;


class Program
{
    static void Main(string[] args)
    {
        //Console.WriteLine(2+2*(22));
        //InlineEvaluate inline = new InlineEvaluate(@"2+2*(22)");
        //Console.WriteLine(inline.Parse(@"2+(2*(22))"));
        InterpreterCore interpreterCore = new InterpreterCore();
        interpreterCore.RegisterComponent(typeof(ClassComponent));
        interpreterCore.RegisterComponent(typeof(LetComponent));
        interpreterCore.RegisterComponent(typeof(GetComponent));
        interpreterCore.Execute(File.ReadAllText("C:\\Users\\Demon\\source\\repos\\DIL\\DIL.Test\\Program.dil"));

    }
}
