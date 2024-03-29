﻿public class PlutoException : System.Exception
{
    public string error_msg;
}

public class LexExceptionUnexpectedSequence : PlutoException
{
    public LexExceptionUnexpectedSequence(string error_msg)
    {
        this.error_msg = "Lexing Exception: " + error_msg;
    }
}

public class ParseExceptionUnexpectedToken : PlutoException
{
    public ParseExceptionUnexpectedToken(string error_msg)
    {
        this.error_msg = "Parsing Exception: " + error_msg;
    }
}

public class RuntimeException : PlutoException
{
    public RuntimeException(string error_msg)
    {
        this.error_msg = "Runtime Exception: "+error_msg;
    }
}

public class ReturnException : PlutoException
{
    public object value;

    public ReturnException(object value)
    {
        this.value = value;
    }
}

public class ContinueException : PlutoException
{
    public ContinueException()
    {   
    }
}

public class InterruptException : PlutoException
{
    public PlutoObject obj;
    public InterruptException(PlutoObject obj, string error_msg)
    {
        this.obj = obj;
        this.error_msg = error_msg;
    }
}