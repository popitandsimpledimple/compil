using System;
using System.IO;

public class Buffer //получает посимвольно сохран и выдаёт буквы
{
    private Pos _pos;
    public StreamReader file;
    protected char? _back;
    protected char _cur;

    protected Pos _curPos = new Pos();

    protected Pos Position { get => _pos; set => _pos = value; }
    protected Pos PositionCur { get => _curPos; set => _curPos = value; }

    protected int Peek()
    {
        return file.Peek();
    }
    protected void Back()
    {
        _back = _cur;
    }

    protected bool IsSpace(char cur)
    {
        return cur is '\n' or '\t' or ' ' or '\r';
    }
    protected void GetNext()
    {
        if (_back != null)
        {
            _cur = (char) _back;
            _back = null;
        }
        else
        {
            _cur = (char)file.Read();
            if (_cur == '\n')
            {
                _curPos.Column = 0;
                _curPos.Line++;
            }
            else
            {
                _curPos.Column++;
            }
        }
    }
    
    public struct Pos
    {
        public int Line = 1;
        public int Column = 0;

        public Pos()
        {
        }
    }
}