using System;
using System.Collections.Generic;
using System.Linq;

public class Interpreter : ExpressionVisitor, StatementVisitor
{
    bool debug;
    // The global scope
    private Scope global_scope = new Scope();
    // The current local scope
    private Scope local_scope;
    // Used to find the depth of locals in the local_scope
    public Dictionary<Expression, int> local_depths = new Dictionary<Expression, int>();
    // Namepsaces containing local_scopes
    private Dictionary<string, Scope> namespaces = new Dictionary<string, Scope>();
    // The current namespace we are using
    private string using_namespace=null;
    // Loader for compiling modules
    public Loader loader;

    private List<Statement> statements;

    public Interpreter()
    {
        debug = false;
        this.loader = new Loader(this);
        this.loader.load_natives();
        this.loader.load_std_lib();
        local_scope = global_scope;
    }

    public void interpret(List<Statement> statements)
    {
        if (debug)
        {
            Console.WriteLine("=== INTERPRETING ===");
        }
        this.statements = statements;
        foreach (Statement statement in statements)
        {
            execute(statement);
        }
    }

    // Add a native object to the global scope
    public void add_native_obj(string name, object obj)
    {
        this.global_scope.define(name, obj);
    }

    // Add an expression, and it's depth in the local scopes to the local_depths
    public void resolve(Expression expression, int local_scope_pos)
    {
        local_depths.Add(expression, local_scope_pos);
    }

    public object evaluate(Expression expression)
    {
        return expression.visit(this);
    }

    public void execute(Statement statement)
    {
        statement.visit(this);
    }

    // Return the value of a variable
    private object lookup_variable(Token name, Expression expression)
    {
        // Check if the value exists in the local_scope enviroment
        if(local_depths.ContainsKey(expression))
        {
            // Return it at the specific scope
            return local_scope.get(name);
        }
        // Check if the namespace we are using has the desired variable
        else if(using_namespace != null && this.namespaces[this.using_namespace].exists(name))
        {
            return this.namespaces[this.using_namespace].get(name);
        }
        // Retrieve from globals
        else
        {
            return global_scope.get(name);
        }
    }

    // Return the truth value of any object
    private bool get_truth(object obj)
    {
        if (obj is int)
        {
            if ((int)obj > 0)
            {
                return true;
            }
            return false;
        }
        else if (obj is double)
        {
            if ((double)obj > 0)
            {
                return true;
            }
            return false;
        }
        else if (obj is string)
        {
            if ((string)obj != "")
            {
                return true;
            }
            return false;
        }
        else if (obj is bool)
        {
            return (bool)obj;
        }
        else
        {
            if (obj != null)
            {
                return true;
            }
            return false;
        }
    }

    public void execute_block(List<Statement> statements, Scope scope)
    {
        Scope previous_scope = this.local_scope;
        // Create a new scope and execute the block
        try
        {
            this.local_scope = scope;
            foreach (Statement statement in statements)
            {
                execute(statement);
            }
        }
        // Restore the original scope
        finally
        {
            this.local_scope = previous_scope;
        }
    }

    // Get the value from a given namespace & identifier
    public object visit_namespace_value(NamespaceValueExpr namespace_value_expr)
    {
        // Check if the given namespace exists
        if(this.namespaces.ContainsKey((string)namespace_value_expr.namespc.name.value))
        {
            return this.namespaces[(string)namespace_value_expr.namespc.name.value].get(namespace_value_expr.identifier);
        }
        throw new RuntimeException("Cannot find namespace '"+(string)namespace_value_expr.namespc.name.value+"'");
    }

    public object visit_assign(AssignExpr assign_expression)
    {
        //Console.WriteLine("visiting assign: " + assign_expression.name.value+", "+local_scope.get(new Token(Token.Type.Identifier,"x")));
        object value = evaluate(assign_expression.value);
        //Console.WriteLine("new value: "+value);
        // If the value is assigning to a local variable, then set it
        if (local_depths.ContainsKey(assign_expression))
        {
            int distance = local_depths[assign_expression];
            local_scope.assign_at(distance, assign_expression.name, value);
        }
        // Else the value is assigning to a global
        else
        {
            global_scope.assign(assign_expression.name, value);
        }
        return value;
    }

    // When assigning to a value in a specific namespace
    public object visit_namespace_assign(AssignNamespaceExpr assign_namespace_expr)
    {
        // Check the given namespace exists
        if (this.namespaces.ContainsKey((string)assign_namespace_expr.identifier.namespc.name.value))
        {
            object value = evaluate(assign_namespace_expr.value);
            // Check the namespace value exists
            if (local_depths.ContainsKey(assign_namespace_expr))
            {
                // Save the old scope and set the current to the new namespace scope
                Scope previous_scope = this.local_scope;
                this.local_scope = this.namespaces[(string)assign_namespace_expr.identifier.namespc.name.value];
                // Assign the value in the scope at the given depth in the new namespace
                int distance = local_depths[assign_namespace_expr];
                local_scope.assign_at(distance, assign_namespace_expr.identifier.identifier, value);
                // Restore the namespace
                this.local_scope = previous_scope;
                return value;
            }
        }
        throw new RuntimeException("Namespace with name '" + (string)assign_namespace_expr.identifier.namespc.name.value + "' doesn't exist");
    }

    public object visit_literal(LiteralExpr literal_expr)
    {
        return literal_expr.value;
    }

    public object visit_list(ListExpr list_expr)
    {
        List<object> list = new List<object>();
        // Evaluate each expression in the list
        foreach (Expression expr in list_expr.values)
        {
            list.Add(evaluate(expr));
        }
        // Create a new list object, with the items as the args
        Callable list_class = (Callable)(this.namespaces["list"].get(new Token(Token.Type.Identifier, "List")));
        // Call the object with the args
        return list_class.call(this, new List<object>() { list });
    }

    public object visit_variable(VariableExpr variable_expr)
    {
        return lookup_variable(variable_expr.name, variable_expr);
    }

    public object visit_unary(UnaryExpr unary_expr)
    {
        object right = evaluate(unary_expr.right);
        switch(unary_expr.op.type)
        {
            case Token.Type.ConnectiveNot:
                {
                    return !get_truth(right);
                }
            case Token.Type.BitwiseNot:
                {
                    return !get_truth(right);
                }
            case Token.Type.Increment:
                {
                    // This needs fixing as we actually need to assign the value
                    return (double)right+1;
                }
            case Token.Type.Decrement:
                {
                    // This needs fixing as we actually need to assign the value
                    return (double)right - 1;
                }
        }
        return null;
    }

    // This needs hugely optimizing...
    public object visit_binary(BinaryExpr binary_expr)
    {
        object left = evaluate(binary_expr.left);
        object right = evaluate(binary_expr.right);
        switch(binary_expr.op.type)
        {
            case Token.Type.BitwiseShiftLeft:
                {
                    if (left is double && right is double)
                    {
                        return (Convert.ToInt32(left) << (Convert.ToInt32(right)));
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left << (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left << Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) << (int)right);
                    }
                    throw new RuntimeException("Can only bit-shift numeric objects");
                }
            case Token.Type.BitwiseShiftRight:
                {
                    if (left is double && right is double)
                    {
                        return (Convert.ToInt32(left) >> (Convert.ToInt32(right)));
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left >> (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left >> Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) >> (int)right);
                    }
                    throw new RuntimeException("Can only bit-shift numeric objects");
                }
            case Token.Type.Plus:
                {
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left + (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left + Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) +(int)right);
                    }
                    else if (left is string && right is string)
                    {
                        return ((string)left + (string)right);
                    }
                    else if (left is double && right is string)
                    {
                        return (left.ToString() + (string)right);
                    }
                    else if (left is string && right is double)
                    {
                        return ((string)left + right.ToString());
                    }
                    throw new RuntimeException("Cannot add non primitive types");
                }
            case Token.Type.Minus:
                {
                    if (left is double && right is double)
                    {
                        return (double)left - (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left - (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left - Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) - (int)right);
                    }
                    throw new RuntimeException("Cannot subtract non primitive types");
                }
            case Token.Type.Multiply:
                {
                    if (left is double && right is double)
                    {
                        return (double)left * (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left * (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left * Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) * (int)right);
                    }
                    else if (left is double && right is string)
                    {
                        return String.Concat(Enumerable.Repeat((string)right, Convert.ToInt32(left)));
                    }
                    else if (left is string && right is double)
                    {
                        return String.Concat(Enumerable.Repeat((string)left, Convert.ToInt32(right)));
                    }
                    throw new RuntimeException("Cannot multiply non primitive types");
                }
            case Token.Type.Divide:
                {
                    if (left is double && right is double)
                    {
                        return (double)left * (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left * (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left * Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) * (int)right);
                    }
                    throw new RuntimeException("Cannot divide non primitive types");
                }
            case Token.Type.Mod:
                {
                    if (left is double && right is double)
                    {
                        return (double)left % (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left % (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left % Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) % (int)right);
                    }
                    throw new RuntimeException("Cannot modulo non primitive types");
                }
            case Token.Type.NoRemainder:
                {
                    if (left is double && right is double)
                    {
                        return Convert.ToInt32(left) / Convert.ToInt32(right);
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left / (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left / Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) / (int)right);
                    }
                    throw new RuntimeException("Cannot modulo non primitive types");
                }
            case Token.Type.Greater:
                {
                    if (left is double && right is double)
                    {
                        return (double)left > (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left > (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left > Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) > (int)right);
                    }
                    else if (left is string && right is string)
                    {
                        return (((string)left).Length > ((string)right).Length);
                    }
                    else if (left is double && right is string)
                    {
                        return (left.ToString().Length > ((string)right).Length);
                    }
                    else if (left is string && right is double)
                    {
                        return (((string)left).Length > right.ToString().Length);
                    }
                    throw new RuntimeException("Cannot compare non primitive types");
                }
            case Token.Type.Less:
                {
                    if (left is double && right is double)
                    {
                        return (double)left < (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left < (int)right);
                    }
                    else if (left is int && right is double)
                    {
                        return ((int)left < Convert.ToInt32(right));
                    }
                    else if (left is double && right is int)
                    {
                        return (Convert.ToInt32(left) < (int)right);
                    }
                    else if (left is string && right is string)
                    {
                        return (((string)left).Length < ((string)right).Length);
                    }
                    else if (left is double && right is string)
                    {
                        return (left.ToString().Length < ((string)right).Length);
                    }
                    else if (left is string && right is double)
                    {
                        return (((string)left).Length < right.ToString().Length);
                    }
                    throw new RuntimeException("Cannot compare non primitive types");
                }
            case Token.Type.GreaterEqual:
                {
                    if (left is double && right is double)
                    {
                        return (double)left >= (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left >= (int)right);
                    }
                    else if (left is string && right is string)
                    {
                        return (((string)left).Length >= ((string)right).Length);
                    }
                    else if (left is double && right is string)
                    {
                        return (left.ToString().Length >= ((string)right).Length);
                    }
                    else if (left is string && right is double)
                    {
                        return (((string)left).Length >= right.ToString().Length);
                    }
                    throw new RuntimeException("Cannot compare non primitive types");
                }
            case Token.Type.LessEqual:
                {
                    if (left is double && right is double)
                    {
                        return (double)left <= (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left <= (int)right);
                    }
                    else if (left is string && right is string)
                    {
                        return (((string)left).Length <= ((string)right).Length);
                    }
                    else if (left is double && right is string)
                    {
                        return (left.ToString().Length <= ((string)right).Length);
                    }
                    else if (left is string && right is double)
                    {
                        return (((string)left).Length <= right.ToString().Length);
                    }
                    throw new RuntimeException("Cannot compare non primitive types");
                }
            case Token.Type.NotEquals:
                {
                    if (left is double && right is double)
                    {
                        return (double)left != (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left != (int)right);
                    }
                    else if (left is string && right is string)
                    {
                        return ((string)left != (string)right);
                    }
                    else if (left is double && right is string)
                    {
                        return (left.ToString() != (string)right);
                    }
                    else if (left is string && right is double)
                    {
                        return ((string)left != right.ToString());
                    }
                    return (get_truth(left) != get_truth(right));
                }
            case Token.Type.Equals:
                {
                    if (left is double && right is double)
                    {
                        return (double)left == (double)right;
                    }
                    else if (left is int && right is int)
                    {
                        return ((int)left == (int)right);
                    }
                    else if (left is string && right is string)
                    {
                        return ((string)left == (string)right);
                    }
                    else if (left is double && right is string)
                    {
                        return (left.ToString() == (string)right);
                    }
                    else if (left is string && right is double)
                    {
                        return ((string)left == right.ToString());
                    }
                    return (get_truth(left) == get_truth(right));
                }
        }
        return null;
    }

    public object visit_connective(ConnectiveExpr connective_expr)
    {
        object left = evaluate(connective_expr.left);
        if(connective_expr.op.type == Token.Type.ConnectiveOr)
        {
            // If left is truthy, return it
            if (get_truth(left)) return left;
        }
        else if(connective_expr.op.type == Token.Type.ConnectiveAnd)
        {
            // If we are on an and, and left is false, then return false
            if (!get_truth(left)) return left;
        }
        return get_truth(evaluate(connective_expr.right));
    }

    public object visit_super(SuperExpr super_expr)
    {
        // Get the superclass and the 'this' object
        PlutoClass superclass = (PlutoClass)local_scope.get(new Token(Token.Type.Super,"super"));
        PlutoObject obj = (PlutoObject)local_scope.get(new Token(Token.Type.This, "this"));
        // Get the method from the superclass
        PlutoFunction method = superclass.find_method(obj, (string)super_expr.identifier.value);
        if(method==null)
        {
            throw new RuntimeException("Undefined method of superclass '"+super_expr.identifier.value+"'");
        }
        return method;
    }

    public object visit_this(ThisExpr this_expr)
    {
        return lookup_variable(new Token(Token.Type.This, "this"), this_expr);
    }

    public object visit_call(CallExpr call_expr)
    {
        // Get the caller object
        object caller = evaluate(call_expr.caller);
        // Get the arguments
        List<object> args = new List<object>();
        foreach(Expression argument in call_expr.args)
        {
            args.Add(evaluate(argument));
        }
        if(!(caller is Callable))
        {
            throw new RuntimeException("Can only call functions, methods and constructors");
        }
        Callable func = (Callable)caller;
        if(args.Count != func.args())
        {
            throw new RuntimeException("Expected " + func.args() + " arguments but got "+ args.Count);
        }
        // Call the object with the args
        return func.call(this, args);
    }

    public object visit_obj_get(ObjGetExpr obj_get_expr)
    {
        object obj = evaluate(obj_get_expr.obj);
        if(obj is PlutoObject)
        {
            return ((PlutoObject)obj).get(obj_get_expr.identifier);
        }
        throw new RuntimeException("Must be object to get member");
    }

    public object visit_obj_set(ObjSetExpr obj_set_expr)
    {
        object obj = evaluate(obj_set_expr.obj);
        if (obj is PlutoObject)
        {
            object value = evaluate(obj_set_expr.value);
            ((PlutoObject)obj).set(obj_set_expr.identifier, value);
            return value;
        }
        throw new RuntimeException("Must be object to set member");
    }

    public object visit_group(GroupExpr group_expr)
    {
        return evaluate(group_expr.group);
    }

    public void visit_compile(CompileStmt Compile_stmt)
    {
        this.loader.compile((string)evaluate(Compile_stmt.code));
    }

    public void visit_using(UsingStmt using_stmt)
    {
        if (this.namespaces.ContainsKey((string)using_stmt.namespace_identifier.value))
        {
            this.using_namespace=((string)using_stmt.namespace_identifier.value);
            return;
        }
        throw new RuntimeException("Cannot find namespace '" + (string)using_stmt.namespace_identifier.value + "'");
    }

    public void visit_import(ImportStmt import_stmt)
    {
        this.loader.import((string)import_stmt.file.value);
    }

    public void visit_return(ReturnStmt return_stmt)
    {
        object value = null;
        if (return_stmt.value != null)
        {
            value = evaluate(return_stmt.value);
        }
        // Throw a new return exception for the loop/function to catch
        throw new ReturnException(value);
    }

    public void visit_continue(ContinueStmt continue_stmt)
    {
        // Throw a new continue exception for the loop/function to catch
        throw new ContinueException();
    }

    // Visit a namespace definition
    public void visit_namespace(NamespaceStmt namespace_stmt)
    {
        // Do we want namespace variables to be indipendent or have the previous scope? [new Scope(this.local_scope);]
        if(this.namespaces.ContainsKey((string)namespace_stmt.name.value))
        {
            throw new RuntimeException("Namespace with name '" + (string)namespace_stmt.name.value + "' already exists");
        }
        // Create a new scope for the given namespace, and execute the namespace code
        this.namespaces.Add((string)namespace_stmt.name.value, new Scope());
        execute_block(namespace_stmt.body, this.namespaces[(string)namespace_stmt.name.value]);
    }

    public void visit_class(ClassStmt class_stmt)
    {
        object superclass = null;
        // Check if we have a superclass, and if so, evaluate that
        if (class_stmt.superclass != null)
        {
            superclass = evaluate(class_stmt.superclass);
            if (!(superclass is PlutoClass))
            {
                throw new RuntimeException("Superclass must be a class type");
            }
        }
        this.local_scope.define((string)class_stmt.name.value, null);
        if (class_stmt.superclass != null)
        {
            // Create a new scope for the superclass class
            local_scope = new Scope(local_scope);
            // Define super as the superclass
            local_scope.define("super", superclass);
        }
        // Create a dictionary of methods for the class
        Dictionary<string, PlutoFunction> methods = new Dictionary<string, PlutoFunction>();
        foreach(FunctionStmt method in class_stmt.methods)
        {
            bool is_constructor = ((string)method.name.value) == (string)class_stmt.name.value;
            PlutoFunction function = new PlutoFunction(method, this.local_scope, is_constructor);
            methods.Add((string)method.name.value, function);
        }
        // Create the class object and assign it to the scope
        PlutoClass p_class = new PlutoClass((string)class_stmt.name.value, (PlutoClass)superclass, methods);
        if(superclass!=null)
        {
            // Reset the scope
            local_scope = local_scope.enclosing_scope;
        }
        local_scope.assign(class_stmt.name, p_class);
    }

    public void visit_function(FunctionStmt function_stmt)
    {
        PlutoFunction func = new PlutoFunction(function_stmt, this.local_scope, false);
        local_scope.define((string)function_stmt.name.value, func);
    }

    public void visit_define(DefStmt define_stmt)
    {
        object value = null;
        if(define_stmt.value != null)
        {
            value = evaluate(define_stmt.value);
        }
        local_scope.define((string)define_stmt.name.value, value);
    }

    public void visit_break(BreakStmt break_stmt)
    {
        throw new ReturnException(null);
    }

    public void visit_trycatch(TryCatchStmt trycatch_stmt)
    {
        try
        {
            execute(trycatch_stmt.try_body);
        }catch(PlutoException exception)
        {
            PlutoFunction function = new PlutoFunction(trycatch_stmt.catch_body, this.local_scope, false);
            // We want to create a new instance of a plutoexception here
            Callable exception_class = (Callable)namespaces["exception"].get(new Token(Token.Type.Identifier, "Exception"));
            function.call(this,
                new List<object>()
                    {
                        exception_class.call(this, new List<object>(){ "[Caught Exception] "+exception.error_msg })
                    }
                ); ;
        }
    }

    public void visit_interrupt(InterruptStmt interrupt_stmt)
    {
        PlutoObject exception_obj = (PlutoObject)evaluate(interrupt_stmt.expression);
        PlutoClass exception = (PlutoClass)namespaces["exception"].get(new Token(Token.Type.Identifier, "Exception"));
        // First check if it extends 'Exception'
        PlutoClass super = exception_obj.the_class.superclass;
        while (super != exception && super != null)
        {
            super = super.superclass;
        }
        if(super != exception)
        {
            throw new RuntimeException("Class must be an Exception to be able to interrupt");
        }
        throw new InterruptException(exception_obj, (string)exception_obj.get(new Token(Token.Type.Identifier, "message")));
    }

    public void visit_for(ForStmt for_stmt)
    {
        execute(for_stmt.counter_definition);
        execute(for_stmt.body);
    }

    public void visit_while(WhileStmt while_stmt)
    {
        while(get_truth(evaluate(while_stmt.condition)))
        {
            try
            {
                execute(while_stmt.body);
            } catch(ReturnException)
            {
                break;
            }
            catch (ContinueException)
            {
                continue;
            }
        }
    }

    public void visit_if(IfStmt if_stmt)
    {
        if(get_truth(evaluate(if_stmt.if_body.Item1)))
        {
            execute(if_stmt.if_body.Item2);
        }else if(if_stmt.elif_body.Count>0)
        {
            foreach (Tuple<Expression, Statement> tup in if_stmt.elif_body)
            {
                if (get_truth(evaluate(tup.Item1)))
                {
                    execute(tup.Item2);
                    break;
                }
            }
        }else if(if_stmt.else_body != null)
        {
            execute(if_stmt.else_body);
        }
    }

    public void visit_block(BlockStmt block_stmt)
    {
        execute_block(block_stmt.statements, new Scope(this.local_scope));
    }

    public void visit_expression(ExprStmt expr_stmt)
    {
        evaluate(expr_stmt.expression);
    }
}
