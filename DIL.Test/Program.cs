using System;

using DIL.Components;
using DIL.Components.ClassComponents;
using DIL.Components.ForLoop;
using DIL.Components.GotoCOmponent;
using DIL.Components.ValueComponent;
using DIL.Components.ValueComponent.Tokens;
using DIL.Core;
using DIL.Middlewares;


class Program
{
    static void Main(string[] args)
    {
        //Console.WriteLine(2 + 2 * (22));
        //InlineEvaluate inline = new InlineEvaluate(@"2*2/(4*6)");
        //Console.WriteLine(inline.Parse());
        InterpreterCore interpreterCore = new InterpreterCore();
        interpreterCore.RegisterComponent(typeof(ClassComponent));
        interpreterCore.RegisterComponent(typeof(LetComponent));
        interpreterCore.RegisterComponent(typeof(IfElseComponent));
        interpreterCore.RegisterComponent(typeof(ForLoopComponent));
        interpreterCore.RegisterComponent(typeof(ForEachComponent));
        interpreterCore.RegisterComponent(typeof(@goto));

        interpreterCore.RegisterComponent(typeof(GetComponent));
        interpreterCore.Execute(File.ReadAllText("C:\\Users\\Demon\\source\\repos\\DIL\\DIL.Test\\Classes.dil"));

    }
}
