using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlutoRuntime
{
    Lexer lexer;
    Parser parser;
    Interpreter interpreter;
    ScopeResolver scope_resolver;

    public PlutoRuntime()
    {
        this.lexer = new Lexer();
        this.parser = new Parser();
        this.interpreter = new Interpreter();
        this.scope_resolver = new ScopeResolver(this.interpreter);
        UnityLoader.load_all(this.interpreter.loader);
    }

    public IEnumerator compile(string lines)
    {
        try
        {
            List<Token> tokens = this.lexer.lex(lines);
            List<Statement> statements = this.parser.parse(tokens);
            this.scope_resolver.resolve(statements);
            this.interpreter.interpret(statements);
        }
        catch (PlutoException exception)
        {
            System.Console.WriteLine(exception.error_msg);
        }
        yield return null;
    }
}
