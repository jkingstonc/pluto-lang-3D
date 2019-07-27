using System.Collections.Generic;

public static class LangNative
{
    public static Dictionary<string, Callable> native_obj = new Dictionary<string, Callable>()
    {
        { "compile_native", new Compile() },
    };
}

public class Compile : Callable
{
    public int args()
    {
        return 1;
    }
    public object call(Interpreter interpreter, List<object> args)
    {
        Lexer lexer = new Lexer();
        Parser parser = new Parser();
        ScopeResolver scope_resolver = new ScopeResolver(interpreter);
        List<Token> tokens = lexer.lex((string)args[0]);
        List<Statement> statements = parser.parse(tokens);
        scope_resolver.resolve(statements);
        interpreter.interpret(statements);
        return null;
    }
}
