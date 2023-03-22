using Lexer;

namespace SimpleParser;

public class SimpleParser //посмотреть лекцию
{
    private readonly Scanner _scan;
    private Lex _curLex;

    public SimpleParser(StreamReader fileReader)
    {
        _scan = new Scanner(fileReader);
        _curLex = _scan.ScannerLex();
    }

    public Node ParseExpression()
    {
        var left = ParseTerm();
        var lex = _curLex;
        while (lex.LexType == LexType.Operator &&
               (Equals(lex.Value, LexOperator.Add) || Equals(lex.Value, LexOperator.Sub)))
        {
            _curLex = _scan.ScannerLex();
            left = new BinOp(lex, left, ParseTerm());
            lex = _curLex;
        }

        return left;
    }

    private Node ParseTerm()
    {
        var left = ParseFactor();
        var lex = _curLex;
        while (lex.LexType == LexType.Operator &&
               (Equals(lex.Value, LexOperator.Mul) || Equals(lex.Value, LexOperator.Div)))
        {
            _curLex = _scan.ScannerLex();
            left = new BinOp(lex, left, ParseFactor());
            lex = _curLex;
        }

        return left;
    }

    private Node ParseFactor()
    {
        var lex = _curLex;

        switch (lex.LexType)
        {
            case LexType.Integer or LexType.Double:
                _curLex = _scan.ScannerLex();
                return new NumberNode(lex);
            case LexType.Identifier:
                _curLex = _scan.ScannerLex();
                return new IdNode(lex);
        }
        if (!Equals(lex.Value, LexSeparator.Lparen))
            throw new SyntaxException(_curLex.Pos, "Factor expected");
        _curLex = _scan.ScannerLex();
        var e = ParseExpression();
        
        if (!Equals(_curLex.Value, LexSeparator.Rparen))
            throw new SyntaxException(_curLex.Pos,"no Rparen");
        _curLex = _scan.ScannerLex();
        return e;
    }

    public abstract class Node
    {
        public Node(Lex lexeme)
        {
            LexCur = lexeme;
        }

        public Lex LexCur { get; set; }

        public abstract void PrintTree(string branchAscii);
    }

    private class BinOp : Node
    {
        public BinOp(Lex lexCur, Node left, Node right) : base(lexCur)
        {
            Left = left;
            Right = right;
        }

        private Node Right { get; }
        private Node Left { get; }

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
            branchAscii = branchAscii.Replace("├───", "│   ");
            branchAscii = branchAscii.Replace("└───", "    ");
            Left.PrintTree(branchAscii + "├───");
            Right.PrintTree(branchAscii + "└───");
        }
        
        public override string ToString()
        {
            return LexCur.Source;
        }
    }

    private class IdNode : Node
    {
        public IdNode(Lex lexeme) : base(lexeme)
        {
        }

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }

        public override string ToString()
        {
            return LexCur.Source;
        }
    }

    private class NumberNode : Node
    {
        public NumberNode(Lex lexeme) : base(lexeme)
        {
        }

        public override string ToString()
        {
            return LexCur.Source;
        }

        public override void PrintTree(string branchAscii)
        {
            Console.WriteLine(branchAscii + LexCur.Source);
        }
    }
}