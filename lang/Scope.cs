﻿using System.Collections.Generic;

public class Scope
{
    // The scope "above" this e.g. if this was a function scope then the enclosing would be a class scope
    public Scope enclosing_scope;

    // The identifiers in this scope
    public Dictionary<string, object> identifiers=new Dictionary<string, object>();

    public Scope()
    {
        this.enclosing_scope = null;
    }

    public Scope(Scope enclosing_scope)
    {
        this.enclosing_scope = enclosing_scope;
    }

    // Define a value in this scope
    public void define(string name, object obj)
    {
        this.identifiers.Add(name, obj);
    }

    // Assign as this scope, which can access the enclosing scopes
    public void assign(Token name, object obj)
    {
        if (identifiers.ContainsKey((string)name.value))
        {
            identifiers[(string)name.value] = obj;
            return;
        }
        if(enclosing_scope!=null)
        {
            enclosing_scope.assign(name, obj);
            return;
        }
        throw new RuntimeException("Cannot find identifier '" + (string)name.value + "'");
    }

    // Assign at a scope distance, used when assigning to scoped variables not in ours
    public void assign_at(int distance, Token name, object obj)
    {
        get_enclosing(distance).identifiers[(string)name.value]=obj;
    }

    // Get a value at any scope depth above
    public object get(Token name)
    {
        if(identifiers.ContainsKey((string)name.value))
        {
            return identifiers[(string)name.value];
        }
        if(enclosing_scope!=null)
        {
            return enclosing_scope.get(name);
        }
        throw new RuntimeException("Cannot find identifier '" + (string)name.value + "'");
    }

    // Get an enclosing scope at a specific depth
    public Scope get_enclosing(int scope_depth)
    {
        Scope s = this;
        for(int i=0;i<scope_depth;i++)
        {
            s = s.enclosing_scope;
        }
        return s;
    }

    // Check if an identifier exists in any enclosing scope
    public bool exists(Token name)
    {
        if (identifiers.ContainsKey((string)name.value))
        {
            return true;
        }
        if (enclosing_scope != null)
        {
            return enclosing_scope.exists(name);
        }
        return false;
    }
}
