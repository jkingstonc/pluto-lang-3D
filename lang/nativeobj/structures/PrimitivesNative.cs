using System;
using System.Collections.Generic;

public static class PrimitivesNative
{
    public static Dictionary<string, Callable> native_obj = new Dictionary<string, Callable>()
    {
        { "int_native", new IntObj() },
    };
}

public class IntObj : Callable
{
    public int args()
    {
        return 1;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        if(args[0] is int)
        {
            return args[0];
        }
        if(args[0] is double)
        {
            return (int)Math.Round((double)args[0]);
        }
        if (args[0] is string)
        {
            return Int32.Parse((string)args[0]);
        }
        return null;
    }
}