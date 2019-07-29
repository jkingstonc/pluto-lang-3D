using System;
using System.Collections.Generic;
using UnityEngine;

public static class UnityNative
{
    public static Dictionary<string, Callable> native_obj = new Dictionary<string, Callable>()
    {
        { "debug_unity_native", new DebugFunc() },
    };
}

public class DebugFunc : Callable
{
    public int args()
    {
        return 1;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        Debug.Log(args[0]);
        return null;
    }
}