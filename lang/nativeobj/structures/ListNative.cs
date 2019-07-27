using System;
using System.Collections.Generic;

public static class ListNative
{
    public static Dictionary<string, Callable> native_obj = new Dictionary<string, Callable>()
    {
        { "createlist_native", new CreateListObj() },
        { "listsize_native", new ListSizeFunc() },
        { "addlist_native", new AddListFunc() },
        { "indexlist_native", new IndexListFunc() },
        { "tostringlist_native", new ToStringLstFunc() },
    };
}

public class CreateListObj : Callable
{
    public int args()
    {
        return 1;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        if(!(args[0] is List<object>))
        {
            throw new RuntimeException("Cannot initialise list with a list...?");
        }
        return args[0];
    }
}

public class ListSizeFunc : Callable
{
    public int args()
    {
        return 1;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        return (int)((List<object>)args[0]).Count;
    }
}

public class AddListFunc : Callable
{
    public int args()
    {
        return 2;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        List<object> list = (List<object>)args[0];
        list.Add(args[1]);
        return list;
    }
}

public class IndexListFunc : Callable
{
    public int args()
    {
        return 2;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        if((int)Math.Round((double)args[1])< ((List<object>)args[0]).Count)
        {
            return ((List<object>)args[0])[(int)Math.Round((double)args[1])];
        }
        throw new RuntimeException("Index is out of range");
    }
}

public class ToStringLstFunc : Callable
{
    public int args()
    {
        return 1;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        return "["+string.Join(", ", ((List<object>)args[0]).ToArray())+"]";
    }
}