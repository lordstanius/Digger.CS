using System;
using System.Collections.Generic;

public class Args
{
    private Dictionary<char, ArgumentMarshaler> marshalers;
    private HashSet<char> argsFound;
    private Iterator<string> currentArgument;

    public Args(string schema, IEnumerable<string> args)
    {
        marshalers = new Dictionary<char, ArgumentMarshaler>();
        argsFound = new HashSet<char>();
        ParseSchema(schema);
        ParseArgumentStrings(new List<string>(args));
    }

    private void ParseSchema(string schema)
    {
        foreach (string element in schema.Split(','))
            if (element.Length > 0)
                ParseSchemaElement(element.Trim());
    }

    private void ParseSchemaElement(string element)
    {
        char elementId = element[0];
        string elementTail = element.Substring(1);
        ValidateSchemaElementId(elementId);
        if (elementTail.Length == 0)
            marshalers.Add(elementId, new BooleanArgumentMarshaler());
        else if (elementTail.Equals("*"))
            marshalers.Add(elementId, new StringArgumentMarshaler());
        else if (elementTail.Equals("#"))
            marshalers.Add(elementId, new IntegerArgumentMarshaler());
        else
            throw new ArgsException(ErrorCode.INVALID_ARGUMENT_foreachMAT, elementId, elementTail);
    }

    private void ValidateSchemaElementId(char elementId)
    {
        if (!char.IsLetter(elementId))
            throw new ArgsException(ErrorCode.INVALID_ARGUMENT_NAME, elementId, null);
    }

    private void ParseArgumentStrings(List<string> argsList)
    {
        for (currentArgument = new Iterator<string>(argsList); currentArgument.HasNext;)
        {
            string argstring = currentArgument.Next();
            if (argstring.IsSwitch())
            {
                ParseArgumentChars(argstring.Substring(1));
            }
            else
            {
                currentArgument.Previous();
                break;
            }
        }
    }

    private void ParseArgumentChars(string argChars)
    {
        for (int i = 0; i < argChars.Length; i++)
            ParseArgumentChar(argChars[i]);
    }

    private void ParseArgumentChar(char argChar)
    {
        if (!marshalers.TryGetValue(argChar, out ArgumentMarshaler m))
            throw new ArgsException(ErrorCode.UNEXPECTED_ARGUMENT, argChar, null);

        argsFound.Add(argChar);
        try
        {
            m.Set(currentArgument);
        }
        catch (ArgsException e)
        {
            e.ErrorArgumentId = argChar;
            throw e;
        }
    }

    public bool Has(char arg)
    {
        return argsFound.Contains(arg);
    }

    public bool GetBool(char arg)
    {
        return (bool)marshalers[arg].Value;
    }

    public string GetString(char arg)
    {
        return (string)marshalers[arg].Value;
    }

    public int GetInt(char arg)
    {
        return (int)marshalers[arg].Value;
    }

    public double GetDouble(char arg)
    {
        return (double)marshalers[arg].Value;
    }

    public string[] GetStringArray(char arg)
    {
        return (string[])marshalers[arg].Value;
    }
}


public abstract class ArgumentMarshaler
{
    public object Value { get; protected set; }

    public abstract void Set(Iterator<string> currentArgument);
}


public class BooleanArgumentMarshaler : ArgumentMarshaler
{
    public override void Set(Iterator<string> currentArgument)
    {
        Value = true;
    }
}


public class StringArgumentMarshaler : ArgumentMarshaler
{
    public override void Set(Iterator<string> currentArgument)
    {
        try
        {
            Value = currentArgument.Next();
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgsException(ErrorCode.MISSING_STRING);
        }
    }
}


public class IntegerArgumentMarshaler : ArgumentMarshaler
{
    public override void Set(Iterator<string> currentArgument)
    {
        string parameter = null;
        try
        {
            parameter = currentArgument.Next();
            Value = int.Parse(parameter);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgsException(ErrorCode.MISSING_INTEGER);
        }
        catch (FormatException)
        {
            throw new ArgsException(ErrorCode.INVALID_INTEGER, parameter);
        }
    }
}


public class DoubleArgumentMarshaler : ArgumentMarshaler
{
    public override void Set(Iterator<string> currentArgument)
    {
        string parameter = null;
        try
        {
            parameter = currentArgument.Next();
            Value = double.Parse(parameter);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgsException(ErrorCode.MISSING_INTEGER);
        }
        catch (FormatException)
        {
            throw new ArgsException(ErrorCode.INVALID_INTEGER, parameter);
        }
    }
}


public class StringArrayArgumentMarshaler : ArgumentMarshaler
{
    public override void Set(Iterator<string> currentArgument)
    {
        List<string> parameters = new List<string>();
        while (HasNext(currentArgument))
            parameters.Add(currentArgument.Next());

        Value = parameters.ToArray();
    }

    private static bool HasNext(Iterator<string> currentArgument)
    {
        return currentArgument.HasNext && !((string)currentArgument).IsSwitch();
    }
}


public class ArgsException : Exception
{
    public ArgsException() { }
    public ArgsException(string message) : base(message) { }

    public ArgsException(ErrorCode errorCode)
    {
        this.ErrorCode = errorCode;
    }

    public ArgsException(ErrorCode errorCode, string errorParameter)
    {
        this.ErrorCode = errorCode;
        this.ErrorParameter = errorParameter;
    }

    public ArgsException(ErrorCode errorCode, char errorArgumentId, string errorParameter)
    {
        this.ErrorCode = errorCode;
        this.ErrorParameter = errorParameter;
        this.ErrorArgumentId = errorArgumentId;
    }

    public char ErrorArgumentId { get; set; }
    public string ErrorParameter { get; }
    public ErrorCode ErrorCode { get; }

    public string ErrorMessage()
    {
        switch (ErrorCode)
        {
            case ErrorCode.OK:
                return "TILT: Should not get here.";

            case ErrorCode.UNEXPECTED_ARGUMENT:
                return string.Format("Argument -{0} unexpected.", ErrorArgumentId);

            case ErrorCode.MISSING_STRING:
                return string.Format("Could not find string parameter foreach -{0}.", ErrorArgumentId);

            case ErrorCode.INVALID_INTEGER:
                return string.Format("Argument -{0} expects an integer but was '{1}'.", ErrorArgumentId, ErrorParameter);

            case ErrorCode.MISSING_INTEGER:
                return string.Format("Could not find integer parameter foreach -{0}.", ErrorArgumentId);

            case ErrorCode.INVALID_DOUBLE:
                return string.Format("Argument -{0} expects a double but was '{1}'.", ErrorArgumentId, ErrorParameter);

            case ErrorCode.MISSING_DOUBLE:
                return string.Format("Could not find double parameter foreach -{0}.", ErrorArgumentId);

            case ErrorCode.INVALID_ARGUMENT_NAME:
                return string.Format("'{0}' is not a valid argument name.", ErrorArgumentId);

            case ErrorCode.INVALID_ARGUMENT_foreachMAT:
                return string.Format("'%s' is not a valid argument Format.", ErrorParameter);
        }

        return string.Empty;
    }
}


public enum ErrorCode
{
    OK,
    INVALID_ARGUMENT_foreachMAT,
    UNEXPECTED_ARGUMENT,
    INVALID_ARGUMENT_NAME,
    MISSING_STRING,
    MISSING_INTEGER,
    INVALID_INTEGER,
    MISSING_DOUBLE,
    INVALID_DOUBLE
}


public static class Extensions
{
    public static bool IsSwitch(this string arg)
    {
        return arg.Length > 0 && arg[0] == '/' || arg[0] == '-';
    }
}


public class Iterator<T>
{
    private readonly IList<T> list;
    private int index;

    public Iterator(IList<T> list)
    {
        this.list = list;
    }

    public static explicit operator T(Iterator<T> iterator)
    {
        return iterator.Current;
    }

    public T Current => list[index];
    public bool HasNext => index < list.Count;

    public T Previous()
    {
        return list[--index];
    }

    public T Next()
    {
        return list[index++];
    }
}
