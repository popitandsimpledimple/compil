using Lexer;
using Parser.Visitor;

namespace Parser;

public partial class Parser : Buffer
{
    private readonly Scanner _scan;
    private Lex _curLex;

    public Parser(StreamReader fileReader)
    {
        _scan = new Scanner(fileReader);
        _curLex = _scan.ScannerLex();
    }

    public interface IAcceptable
    {
        public void Accept(IVisitor visitor);
    }

    public abstract class Node : IAcceptable
    {
        public abstract void Accept(IVisitor visitor);
    }

   public class KeywordNode : Node // типы переменных при парсере и синтанал
    {
        public KeywordNode(Lex lexCur)
        {
            LexCur = lexCur;
        }
        public Lex LexCur { get; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ProgramNode : Node
    {
        public ProgramNode(IdNode? name, BlockNode block)
        {
            Name = name;
            Block = block;
        }

        public IdNode? Name { get; }
        public BlockNode Block { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public ProgramNode Program()
    {
        IdNode? name = null;
        if (_curLex.Is(LexKeywords.PROGRAM))
        {
            Eat();
            name = Id();
            Require(LexSeparator.Semicolom);
        }

        BlockNode block = Block();
        Require(LexSeparator.Dot);
        return new ProgramNode(name, block);
    }

    public void Require(LexOperator op, bool eat = true)
    {
        if (_curLex.Is(op))
        {
            if (eat)
            {
                Eat();
            }

            return;
        }

        throw new SyntaxException(_curLex.Pos, $"Expected '{op}' Found '{_curLex.Value}'");
    }
    public void Require(LexSeparator sep, bool eat = true)
    {
        if (_curLex.Is(sep))
        {
            if (eat)
            {
                Eat();
            }

            return;
        }

        throw new SyntaxException(_curLex.Pos, $"Expected '{sep}' Found '{_curLex.Value}'");
    }

    public void Require(LexKeywords keyword, bool eat = true)
    {
        if (_curLex.Is(keyword))
        {
            if (eat)
            {
                Eat();
            }

            return;
        }

        throw new SyntaxException(_curLex.Pos, $"Expected '{keyword}' Found '{_curLex.Value}'");
    }

    public void Eat()
    {
        _curLex = _scan.ScannerLex();
    }

    public IdNode Id()
    {
        if (!_curLex.Is(LexType.Identifier))
        {
            throw new SyntaxException(_curLex.Pos, "Expect Identifier");
        }

        var lex = _curLex;
        Eat();
        return new IdNode(lex);
    }

    public KeywordNode Keyword()
    {
        var lex = _curLex;
        if (!_curLex.Is(LexType.Keyword))
        {
            throw new Exception("Expect Keyword");
        }
        Eat();
        return new KeywordNode(lex);
    }
}