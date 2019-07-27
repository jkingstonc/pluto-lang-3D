using System;
using System.Collections.Generic;
using System.Threading;

public static class TimeNative
{
    public static Dictionary<string, Callable> native_obj = new Dictionary<string, Callable>()
    {
        { "time_native", new GetTimeFunc() },
        { "timediff_native", new TimeDiffFunc() },
        { "timesleep_native", new TimeSleepFunc() },
    };
}

public class GetTimeFunc : Callable
{
    public int args()
    {
        return 0;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        return DateTime.Now;
    }
}

public class TimeDiffFunc : Callable
{
    public int args()
    {
        return 2;
    }

    public object call(Interpreter interpreter, List<object> args)
    {
        return ((DateTime)args[0]).Subtract(((DateTime)args[1])).TotalSeconds;
    }
}

public class TimeSleepFunc : Callable
{
    public int args()
    {
        return 1;
    }

    public object call(Interpreter interpreter, List<object> args)
    {
        Thread.Sleep(Convert.ToInt32(args[0]));
        return null;
    }
}