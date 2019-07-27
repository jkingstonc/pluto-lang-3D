using System;
using System.Collections.Generic;

public static class MathsNative
{
    public static Dictionary<string, Callable> native_obj = new Dictionary<string, Callable>()
    {
        { "power_native", new PowerFunc() },
        { "log_native", new LogFunc() },
    };
}

public class PowerFunc : Callable
{
    public int args()
    {
        return 2;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        return Math.Pow((double)args[0], (double)args[1]);
    }
}

public class LogFunc : Callable
{
    public int args()
    {
        return 2;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        return Math.Log((double)args[1], (double)args[0]);
    }
}