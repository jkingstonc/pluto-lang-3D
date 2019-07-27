using System;
using System.Collections.Generic;

public static class IONative
{
    public static Dictionary<string, Callable> native_obj = new Dictionary<string, Callable>()
    {
        { "print_native", new PrintFunc() },
        { "get_native", new GetFunc() },
    };
}

public class PrintFunc : Callable
{
    public int args()
    {
        return 1;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        Console.WriteLine(args[0]);
        return null;
    }
}

public class GetFunc : Callable
{
    public int args()
    {
        return 0;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        return Console.ReadLine();
    }
}