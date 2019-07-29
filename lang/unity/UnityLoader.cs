using System;
using System.Collections.Generic;

static class UnityLoader
{
    private static List<UnityCallable> callables = new List<UnityCallable>();

    public static void load(UnityCallable unity_callable)
    {
        UnityLoader.callables.Add(unity_callable);
    }

    public static void load_all(Loader loader)
    {
        foreach (UnityCallable callable in UnityLoader.callables)
        {
            loader.load_native(callable.name, callable.callable);
        }
    }
}
