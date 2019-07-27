using System.Collections.Generic;
using System.IO;
using System.Text;

class Loader
{
    //public static string pluto_env = "C:\\Program Files\\Pluto\\lang\\";
    public static string pluto_env = "F:\\OneDrive - Lancaster University\\programming\\c#\\visual studio projects\\pluto\\Pluto\\ConsoleApp1\\";
    //public static string pluto_env = "c:\\users\\44778\\OneDrive - Lancaster University\\programming\\c#\\visual studio projects\\pluto\\Pluto\\ConsoleApp1\\";
    // Path for native library
    public static string native_path = pluto_env + "nativeobj\\";
    // Path for all import modules
    public static string modules_path = pluto_env + "modules\\";
    // Path for the std package inside the modules directory
    public static string std_path = modules_path + "std\\";

    private Interpreter interpreter;
    private List<string> loaded_modules = new List<string>();

    public Loader(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    // Load the standard library to the interpreter
    public void load_std_lib()
    {
        import("std");
    }

    // Import a module, with a given path to a directory, or specific file
    public void import(string module)
    {
        // Replace the .'s with path seperators
        module = module.Replace(".", "\\");
        string search = modules_path + module;
        // First check the modules path
        if (Directory.Exists(search))
        {
            import_path(search);
            return;
        }else if (File.Exists(search + ".pluto"))
        {
            import_module(search + ".pluto");
            return;
        }
        // Else, check the local files to see if it is a custom module
        string custom_search = Directory.GetCurrentDirectory() + module;
        if (Directory.Exists(custom_search))
        {
            import_path(custom_search);
            return;
        }
        else if (File.Exists(custom_search + ".pluto"))
        {
            import_module(custom_search + ".pluto");
            return;
        }
        throw new RuntimeException("Couldn't find module '"+module+"'");
    }

    // Load all native libraries
    // Ideally, this would be done by the library itself, however execution order doesn't allow this as they don't
    // get a change to load themself's
    public void load_natives()
    {
        foreach (KeyValuePair<string, Callable> callable_native in LangNative.native_obj)
        {
            this.interpreter.add_native_obj(callable_native.Key, callable_native.Value);
        }
        foreach (KeyValuePair<string, Callable> callable_native in IONative.native_obj)
        {
            this.interpreter.add_native_obj(callable_native.Key, callable_native.Value);
        }
        foreach (KeyValuePair<string, Callable> callable_native in MathsNative.native_obj)
        {
            this.interpreter.add_native_obj(callable_native.Key, callable_native.Value);
        }
        foreach (KeyValuePair<string, Callable> callable_native in ListNative.native_obj)
        {
            this.interpreter.add_native_obj(callable_native.Key, callable_native.Value);
        }
        foreach (KeyValuePair<string, Callable> callable_native in PrimitivesNative.native_obj)
        {
            this.interpreter.add_native_obj(callable_native.Key, callable_native.Value);
        }
        foreach (KeyValuePair<string, Callable> callable_native in TimeNative.native_obj)
        {
            this.interpreter.add_native_obj(callable_native.Key, callable_native.Value);
        }
    }

    // Import a given module file
    private void import_module(string file)
    {
        // Check if we haven't already loaded it
        if (!loaded_modules.Contains(file))
        {
            loaded_modules.Add(file);
            string contents = File.ReadAllText(file, Encoding.UTF8);
            compile(contents);
        }
    }

    // Recursively import all modules in a directory
    private void import_path(string directory_path)
    {
        var paths = Directory.GetDirectories(directory_path);
        var files = Directory.GetFiles(directory_path);
        foreach(string path in paths)
        {
            import_path(path);
        }
        foreach (string file in files)
        {
            import_module(file);
        }
    }

    // Compile a module to the interpreter
    private void compile(string code)
    {
        Lexer lexer = new Lexer();
        Parser parser = new Parser();
        ScopeResolver scope_resolver = new ScopeResolver(this.interpreter);
        List<Token> tokens = lexer.lex(code);
        List<Statement> statements = parser.parse(tokens);
        scope_resolver.resolve(statements);
        this.interpreter.interpret(statements);
    }
}