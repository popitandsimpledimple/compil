using System;
using System.Globalization;
using System.IO;

namespace Lexer;

public class Scanner : Buffer
{
    private static string _buf;
    private static string _bufString;
    private States _state; // машина состо€ни€ (нет)
    public Lex Lexeme;
    
    public Scanner(StreamReader fileReader)
    {
        file = fileReader;
    }
    private void SkipCommentSpace()
    {
        while (true)
        {
            if (_cur == '/')
            {
                if (Peek() == '/')
                    while (Peek() != '\n' && Peek() > 0)
                    {
                        GetNext();
                    }
                else break;
                GetNext();
            }
            else if (_cur == '(')
            {
                if (Peek() == '*')
                {
                    GetNext();
                    while (Peek() != ')' || _cur != '*')
                    {
                        if (file.EndOfStream)
                        {
                            _curPos.Column++;
                            throw new LexException(PositionCur, "Ќеожиданный конец файла"); // нет продолжени€
                        }
                        GetNext();
                    }
                    GetNext();
                    GetNext();
                }
                else break;
            }
            else if (_cur == '{')
            {
                while (_cur != '}')
                {
                    if (file.EndOfStream)
                    {
                        _curPos.Column++;
                        throw new LexException(PositionCur, "Ќеожиданный конец файла");
                    }
                    GetNext();
                }
                GetNext();
            }
            else if (IsSpace(_cur))
            {
                SkipSpace();
            } 
            else break;
        }
    }
    private void SkipSpace() //скип пробела
    {
        while (IsSpace(_cur))
        {
            GetNext();
        }
    }
    private static void ClearBuf() //очистка буфера
    {
        _buf = "";
    }
    private static void AddBuf(char symbol) //добав символа
    {
        _buf += symbol;
    }
    private object SearchKeyword() //бегает по енуму и скипывает если не нашЄл
    {
        if (Enum.TryParse(_buf, true, out LexKeywords keyWords))
            return Enum.Parse(typeof(LexKeywords), _buf, true);
        return "";
    }
    private void AddLex(LexType id, object value, string source) // считывание лексем по одному
    {
        Lexeme = new Lex(id, source, value, Position);
    }
    private bool IsDigit(char c, int baseDigit)
    {
        return baseDigit switch
        {
            10 => c is >= '0' and <= '9',
            16 => c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f',
            8 => c is >= '0' and <= '7',
            2 => c is >= '0' and <= '1',
            _ => throw new LexException(PositionCur, "Invalid integer expression")
        };
    }
    private int Getbase(char c)
    {
        return c switch
        {
            '%' => 2,
            '&' => 8,
            '$' => 16,
            _ => 10
        };
    }
    private void State() // стейт машина
    {
        switch (_state)
        {
            case States.Num:
                var baseNum = Getbase(_cur);
                while (_state == States.Num)
                {
                    while (IsDigit((char)Peek(), baseNum))
                    {
                        GetNext();
                        AddBuf(_cur);
                        if (Convert.ToInt64(_buf, baseNum) > Int32.MaxValue)
                        {
                            throw new LexException(Position, "Type overflow");
                        }
                    }
                    if (Peek() == '.'|| char.ToLower((char)Peek()) == 'e')
                    {
                        GetNext();
                        if (Peek() != '.')
                        {
                            var originalBuf = _buf;
                            _buf = Convert.ToInt64(_buf, baseNum).ToString();
                            AddBuf(_cur);
                            originalBuf += _cur;
                            string fracDigit = "";
                            while (IsDigit((char) Peek(), 10))
                            {
                                GetNext();
                                AddBuf(_cur);
                                fracDigit += _cur;
                            }

                            originalBuf += fracDigit;

                            if (char.ToLower((char) Peek()) == 'e')
                            {
                                GetNext();
                                AddBuf(_cur);
                                originalBuf += _cur;
                            }
                            if (char.ToLower(_cur) == 'e')
                            {
                                if (Peek() == '-' || Peek() == '+')
                                {
                                    GetNext();
                                    AddBuf(_cur);
                                    originalBuf += _cur;
                                }

                                string expDigit = "";

                                while (IsDigit((char) Peek(), 10))
                                {
                                    GetNext();
                                    expDigit += _cur;
                                }

                                if (expDigit == "")
                                {
                                    if (_buf.Contains('.') && fracDigit == "")
                                        throw new LexException(Position, "Illegal floating point constant");
                                    string illegal = 6 < Peek() && Peek() < 14 ? $"#{Peek()}" : ((char) Peek()).ToString();
                                    throw new LexException(Position, $"Illegal character '{illegal}'");
                                }
                                _buf += expDigit;
                                originalBuf += expDigit;
                            }
                            AddLex(LexType.Double, Convert.ToDouble(_buf),
                                baseNum == 16 ? "$" + originalBuf : baseNum == 8 ? "&" + originalBuf : baseNum == 2 ? "%" + originalBuf : originalBuf);// воиспроизводит ориг число до перевода в 10сс
                            _state = States.Fin;
                            break;
                        }
                        Back();
                    }
                
                    AddLex(LexType.Integer, Convert.ToInt32(_buf, baseNum),
                        baseNum == 16 ? "$" + _buf : baseNum == 8 ? "&" + _buf : baseNum == 2 ? "%" + _buf : _buf);
                    _state = States.Fin;
                }
                break;

            case States.Id:
                while (_state == States.Id)
                    if ((char.IsLetterOrDigit((char)Peek()) || Peek() == '_'))
                    {
                        GetNext();
                        AddBuf(_cur);
                    }
                    else
                    {
                        var searchKeyword = SearchKeyword();
                        if (searchKeyword.ToString().ToLower() != "")
                            AddLex(LexType.Keyword, searchKeyword.ToString().ToUpper(), _buf);
                        else
                            AddLex(LexType.Identifier, _buf, _buf);

                        _state = States.Fin;
                    }

                break;

            case States.Opr:
                switch (_cur)
                {
                    case '-':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexOperator.AssignSub, _buf);
                        }
                        else
                        {
                            AddLex(LexType.Operator, LexOperator.Sub, _buf);
                        }

                        break;
                    case '+':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexOperator.AssignAdd, _buf);
                        }
                        else
                        {
                            AddLex(LexType.Operator, LexOperator.Add, _buf);
                        }

                        break;
                    case '*':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexOperator.AssignMul, _buf);
                        }
                        else
                        {
                            AddLex(LexType.Operator, LexOperator.Mul, _buf);
                        }

                        break;
                    case '/':
                        if (Peek() == '=')
                        {
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexOperator.AssignDiv, _buf);
                        }
                        else
                        {
                            AddLex(LexType.Operator, LexOperator.Div, _buf);
                        }

                        break;
                }

                _state = States.Fin;
                break;

            case States.Chr:
                while (true)
                {
                    var _localChar = "";
                    if (!char.IsDigit((char)Peek()))
                        throw new LexException(PositionCur, $"Illegal character '{(char)Peek()}'");

                    while (char.IsDigit((char)Peek()))
                    {
                        GetNext();
                        _localChar += _cur;
                        if (long.Parse(_localChar) > 65535)
                        {
                            throw new LexException(PositionCur, $"Illegal character '{_cur}'");
                        }
                    }
                    _bufString += '#' + _localChar;
                    AddBuf((char) long.Parse(_localChar));
                    if (Peek() == '#')
                        GetNext();
                    else
                        break;
                }

                if (Peek() == '\'') 
                {
                    GetNext();
                    _state = States.Str;
                    State();
                }
                else if (_buf.Length == 1)
                {
                    AddLex(LexType.Char, _buf, _bufString);
                    _bufString = "";
                }
                else
                {
                    AddLex(LexType.String, _buf, _bufString);
                    _bufString = "";
                }

                break;

            case States.Str:
                var _localString = "";
                while (Peek() != (char) 39)
                {
                    if (file.EndOfStream)
                        throw new LexException(PositionCur, "Unclosed string constant lexeme");
                    if (Peek() == '\n')
                        throw new LexException(PositionCur, "String exceeds line");
                    GetNext();
                    AddBuf(_cur);
                    _localString += _cur;
                }

                _bufString += (char) 39 + _localString + (char) 39;
                GetNext();

                if (Peek() == (char) 39)
                {
                    GetNext();
                    _buf += (char) 39;
                    State();
                }
                else if (Peek() == '#')
                {
                    _state = States.Chr;
                    GetNext();
                    State();
                }
                else
                {
                    AddLex(LexType.String, _buf, _bufString);
                    _bufString = "";
                    _state = States.Fin;
                }

                break;

            case States.Eof:
                Lexeme = new Lex(LexType.Eof, "", "", Position);
                break;

            case States.Er:
                throw new LexException(PositionCur, $"Illegal character '{_cur}'");
        }
    }
    public Lex ScannerLex()
    {
        _state = States.Fin;
        GetNext();
        SkipCommentSpace();
        Position = PositionCur;
        ClearBuf();
        if (char.IsLetter(_cur) || _cur == '_' || (_cur == '&' && char.IsLetter((char)Peek())))
        {
            AddBuf(_cur);
            _state = States.Id;
        }
        else if (char.IsDigit(_cur))
        {
            AddBuf(_cur);
            _state = States.Num;
        }
        else
        {
            switch (_cur)
            {
                case '%':
                case '$':
                case '&':
                    if (!IsDigit((char) Peek(), Getbase(_cur)))
                        throw new LexException(PositionCur, "Invalid integer expression");
                    _state = States.Num;
                    break;
                case '#':
                    _state = States.Chr;
                    break;
                case (char) 39:
                    _state = States.Str;
                    break;
                case ';':
                    AddLex(LexType.Separator, LexSeparator.Semicolom, _cur.ToString());
                    break;
                case '=':
                    AddLex(LexType.Operator, LexOperator.Equal, _cur.ToString());
                    break;
                case ',':
                    AddLex(LexType.Separator, LexSeparator.Comma, _cur.ToString());
                    break;
                case ')':
                    AddLex(LexType.Separator, LexSeparator.Rparen, _cur.ToString());
                    break;
                case '[':
                    AddLex(LexType.Separator, LexSeparator.Lbrack, _cur.ToString());
                    break;
                case ']':
                    AddLex(LexType.Separator, LexSeparator.Rbrack, _cur.ToString());
                    break;
                case '(':
                    AddLex(LexType.Separator, LexSeparator.Lparen, _cur.ToString());
                    break;
                case ':':
                    AddBuf(_cur);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexOperator.Assign, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexSeparator.Colon, _cur.ToString());
                            break;
                    }

                    break;
                case '<':
                    AddBuf(_cur);
                    switch (Peek())
                    {
                        case '>':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexOperator.NoEqual, _buf);
                            break;
                        case '=':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexOperator.LessEqual, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexOperator.Less, _cur.ToString());
                            break;
                    }

                    break;
                case '>':
                    AddBuf(_cur);
                    switch (Peek())
                    {
                        case '=':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Operator, LexOperator.MoreEqual, _buf);
                            break;
                        default:
                            AddLex(LexType.Operator, LexOperator.More, _cur.ToString());
                            break;
                    }

                    break;
                case '.':
                    AddBuf(_cur);
                    switch (Peek())
                    {
                        case '.':
                            GetNext();
                            AddBuf(_cur);
                            AddLex(LexType.Separator, LexSeparator.Doubledot, _buf);
                            break;
                        default:
                            AddLex(LexType.Separator, LexSeparator.Dot, _buf);
                            break;
                    }

                    break;
                case '+' or '-' or '/' or '*':
                    AddBuf(_cur);
                    _state = States.Opr;
                    break;
                case (char)65535 when file.EndOfStream:
                    _state = States.Eof;
                    break;
                default:
                    _state = States.Er;
                    break;
            }
        }

        State();
        return Lexeme;
    }
}