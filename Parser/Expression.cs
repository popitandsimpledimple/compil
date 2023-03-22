using Lexer;
using Parser.Sym;
using Parser.Visitor;

namespace Parser;

public partial class Parser
{
    public abstract class ExpressionNode : Node
    {
        public ExpressionNode(Lex lexCur)
        {
            LexCur = lexCur;
            LValue = true;
        }
        public Lex LexCur { get; }
        public SymType SymType { get; set; } = null!;
        public bool LValue { get; set; }
    }

    public abstract class VarRefNode : ExpressionNode
    {
        public VarRefNode(Lex lexCur) : base(lexCur)
        {
            LexCur = lexCur;
            LValue = true;
        }
        public Lex LexCur { get; }
        public bool LValue { get; set; }
    }

    public class UnOpExpressionNode : ExpressionNode
    {
        public UnOpExpressionNode(Lex op, ExpressionNode operand) : base(op)
        {
            Op = op;
            Operand = operand;
        }

        public Lex Op { get; }

        public ExpressionNode Operand { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class BinOpExpressionNode : ExpressionNode
    {
        public BinOpExpressionNode(Lex op, ExpressionNode left, ExpressionNode right) : base(op)
        {
            Op = op;
            Left = left;
            Right = right;
        }

        public Lex Op { get; }
        public ExpressionNode Right { get; }
        public ExpressionNode Left { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class RelOpExpressionNode : ExpressionNode
    {
        public RelOpExpressionNode(Lex op, ExpressionNode left, ExpressionNode right) : base(op)
        {
            Op = op;
            Left = left;
            Right = right;
        }

        public Lex Op { get; }

        public ExpressionNode Right { get; }
        public ExpressionNode Left { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class WriteCallNode : CallNode
    {
        public WriteCallNode(IdNode name, List<ExpressionNode> args, bool newLine) : base(name, args)
        {
            NewLine = newLine;
        }
        public bool NewLine { get; }
    }
    public class ReadCallNode : CallNode
    {
        public ReadCallNode(IdNode name, List<ExpressionNode> args, bool newLine) : base(name, args)
        {
            NewLine = newLine;
        }

        public bool NewLine { get; }
    }

    public class ArrayAccess : VarRefNode
    {
        public ArrayAccess(VarRefNode arrayId, ExpressionNode arrayExp) : base(arrayId.LexCur)
        {
            ArrayId = arrayId;
            ArrayExp = arrayExp;
        }

        public VarRefNode ArrayId { get; }
        public ExpressionNode ArrayExp { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class RecordAccess : VarRefNode
    {
        public RecordAccess(VarRefNode recordVarRef, IdNode field) : base(recordVarRef.LexCur)
        {
            RecordVarRef = recordVarRef;
            Field = field;
        }

        public VarRefNode RecordVarRef { get; }
        public IdNode Field { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class CastNode : ExpressionNode
    {
        public CastNode(ExpressionNode exp, SymType symType) : base(exp.LexCur)
        {
            SymType = symType;
            LValue = exp.LValue;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class CallNode : VarRefNode
    {
        public CallNode(VarRefNode name, List<ExpressionNode> args) : base(name.LexCur)
        {
            Name = name;
            Args = args;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public List<ExpressionNode> Args { get; }
        public VarRefNode Name { get; }
    }

    public class IdNode : VarRefNode
    {
        public IdNode(Lex lexCur) : base(lexCur)
        {
            LexCur = lexCur;
        }

        public Lex LexCur { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return LexCur.Value.ToString()!.ToLower();
        }
    }

    public class BooleanNode : ExpressionNode
    {
        public BooleanNode(Lex lexCur) : base(lexCur)
        {
            LexCur = lexCur;
        }

        public Lex LexCur { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class StringNode : ExpressionNode
    {
        public StringNode(Lex lexCur) : base(lexCur)
        {
            LexCur = lexCur;
        }

        public Lex LexCur { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return LexCur.Value.ToString()!.ToLower();
        }
    }

    public class CharNode : ExpressionNode 
    {
        public CharNode(Lex lexCur) : base(lexCur)
        {
            LexCur = lexCur;
        }

        public Lex LexCur { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return LexCur.Value.ToString()!.ToLower();
        }
    }

    public class NumberExpressionNode : ExpressionNode
    {
        public NumberExpressionNode(Lex lexCur) : base(lexCur)
        {
            LexCur = lexCur;
        }

        public override string ToString()
        {
            return LexCur.Value.ToString()!.ToLower();
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        public Lex LexCur { get; }
    }

    private CallNode Stream()
    {
        var lex = _curLex;
        Eat();
        var args = new List<ExpressionNode>();
        if (_curLex.Is(LexSeparator.Lparen))
        {
            Eat();
            args = ExpressionList();
            Require(LexSeparator.Rparen);
        }

        if (lex.Is(LexKeywords.WRITE, LexKeywords.WRITELN))
            return new WriteCallNode(new IdNode(lex), args, lex.Is(LexKeywords.WRITELN));
        return new ReadCallNode(new IdNode(lex), args, lex.Is(LexKeywords.READLN));
    }

    private ExpressionNode Expression()
    {
        var left = SimpleExpression();
        var lex = _curLex;
        if (lex.Is(LexOperator.Equal, LexOperator.Less, LexOperator.More, LexOperator.NoEqual, LexOperator.LessEqual,
                LexOperator.MoreEqual))
        {
            Eat();
            left = new RelOpExpressionNode(lex, left, SimpleExpression());
        }

        return left;
    }

    private ExpressionNode SimpleExpression()
    {
        var left = Term();
        var lex = _curLex;
        while (lex.Is(LexOperator.Add, LexOperator.Sub) || lex.Is(LexKeywords.OR, LexKeywords.XOR))
        {
            Eat();
            left = new BinOpExpressionNode(lex, left, Term());
            lex = _curLex;
        }

        return left;
    }

    private ExpressionNode Term()
    {
        var left = SimpleTerm();
        var lex = _curLex;
        while (lex.Is(LexOperator.Mul, LexOperator.Div) || lex.Is(LexKeywords.MOD, LexKeywords.AND, LexKeywords.SHR,
                   LexKeywords.SHL, LexKeywords.DIV))
        {
            Eat();
            left = new BinOpExpressionNode(lex, left, SimpleTerm());
            lex = _curLex;
        }

        return left;
    }

    private ExpressionNode SimpleTerm()
    {
        if (!_curLex.Is(LexOperator.Add, LexOperator.Sub) && !_curLex.Is(LexKeywords.NOT)) return Factor();
        var op = _curLex;
        Eat();
        return new UnOpExpressionNode(op, SimpleTerm());
    }

    private ExpressionNode Factor()
    {
        var lex = _curLex;

        if (lex.Is(LexType.String))
        {
            Eat();
            return new StringNode(lex);
        }

        if (lex.Is(LexType.Char))
        {
            Eat();
            return new CharNode(lex);
        }

        if (lex.Is(LexType.Integer) || lex.Is(LexType.Double))
        {
            Eat();
            return new NumberExpressionNode(lex);
        }

        if (lex.Is(LexType.Keyword))
            if (lex.Is(LexKeywords.TRUE, LexKeywords.FALSE))
            {
                Eat();
                return new BooleanNode(lex);
            }

        if (lex.Is(LexType.Identifier)) return VarRef();
        if (lex.Is(LexKeywords.WRITE, LexKeywords.READ, LexKeywords.WRITELN, LexKeywords.READLN)) return Stream();
        if (lex.Is(LexSeparator.Lparen))
        {
            Eat();
            var e = Expression();
            Require(LexSeparator.Rparen);
            return e;
        }

        throw new SyntaxException(lex.Pos, "factor expected");
    }

    private VarRefNode VarRef()
    {
        var left = Id() as VarRefNode;
        var lex = _curLex;
        while (true)
            if (lex.Is(LexSeparator.Lbrack))
            {
                //ArrayAccess
                Eat();
                var args = new List<ExpressionNode>();
                if (!lex.Is(LexSeparator.Rbrack)) args = ExpressionList();

                foreach (var index in args) left = new ArrayAccess(left, index);

                Require(LexSeparator.Rbrack);
                lex = _curLex;
            }
            else if (lex.Is(LexSeparator.Dot))
            {
                //RecordAccess
                Eat();
                left = new RecordAccess(left, Id());
                lex = _curLex;
            }
            else if (lex.Is(LexSeparator.Lparen))
            {
                //FunctionCall
                Eat();
                var args = new List<ExpressionNode>();

                if (!_curLex.Is(LexSeparator.Rparen)) args = ExpressionList();

                left = new CallNode(left, args);

                Require(LexSeparator.Rparen);

                lex = _curLex;
            }
            else
            {
                break;
            }

        return left;
    }
}