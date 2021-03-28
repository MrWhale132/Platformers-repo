using System;

public class NegativeItemQuantityException : Exception
{
    public NegativeItemQuantityException()
    {

    }

    public NegativeItemQuantityException(string message) : base(message)
    {

    }

    public NegativeItemQuantityException(string message, Exception inner) : base(message, inner)
    {

    }
}