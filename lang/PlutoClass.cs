using System.Collections.Generic;

public class PlutoClass : Callable
{
    private string name;
    public PlutoClass superclass;
    public Dictionary<string, PlutoFunction> methods;

    public PlutoClass(string name, PlutoClass superclass, Dictionary<string, PlutoFunction> methods)
    {
        this.name = name;
        this.superclass = superclass;
        this.methods = methods;
    }

    // Get the number of arguments the constructor takes
    public int args()
    {
        // Check if the class has a constructor
        if(this.methods.ContainsKey(this.name) && this.methods[this.name] != null)
        {
            return this.methods[this.name].args();
        }
        return 0;
    }

    // Create an instance of the class here
    public object call(Interpreter interpreter, List<object> args)
    {
        PlutoObject obj = new PlutoObject(this);
        // Check if the class has a constructor
        if (this.methods.ContainsKey(this.name))
        {
            // Bind the function to this instance and call the constructor
            this.methods[this.name].bind(obj).call(interpreter, args);
        }
        return obj;
    }

    public PlutoFunction find_method(PlutoObject obj, string identifier)
    {
        // Find method in this class
        if (this.methods.ContainsKey(identifier))
        {
            return methods[identifier].bind(obj);
        }
        // Find method in superclass
        if (superclass != null)
        {
            return superclass.find_method(obj, identifier);
        }
        return null;
    }
}