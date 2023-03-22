using System;

public class LexException : Exception // отлов ошибки и вывод её поз. и сама ошибка
{
    public LexException(Buffer.Pos pos, string message)
        : base($"({pos.Line},{pos.Column}) {message}")
    { }
}
public class SyntaxException : Exception
{
    public SyntaxException(Buffer.Pos pos, string message)
        : base($"({pos.Line},{pos.Column}) {message}")
    {
    }
}
public class SemanticException : Exception
{
    public SemanticException(Buffer.Pos pos, string message)
        : base($"({pos.Line},{pos.Column}) {message}")
    {
    }
}