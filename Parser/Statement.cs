using Lexer;
using Parser.Visitor;

namespace Parser;

public partial class Parser
{
    public abstract class StatementNode : Node
    {
    }

    public class CompoundStatementNode : StatementNode
    {
        public CompoundStatementNode(List<StatementNode> states)
        {
            States = states;
        }

        public List<StatementNode> States { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class IfStatementNode : StatementNode
    {
        public IfStatementNode(ExpressionNode exp, StatementNode stateThen, StatementNode stateElse)
        {
            Exp = exp;
            StateThen = stateThen;
            StateElse = stateElse;
        }

        public ExpressionNode Exp { get; set; }
        public StatementNode StateThen { get; set; }
        public StatementNode StateElse { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ForStatementNode : StatementNode
    {
        public ForStatementNode(VarRefNode id, ExpressionNode expFor, KeywordNode to, ExpressionNode expTo,
            StatementNode state)
        {
            Id = id;
            ExpFor = expFor;
            ExpTo = expTo;
            To = to;
            State = state;
        }

        public VarRefNode Id { get; }
        public ExpressionNode ExpFor { get; }
        public ExpressionNode ExpTo { get; }
        public KeywordNode To { get; }
        public StatementNode State { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class WhileStatementNode : StatementNode
    {
        public WhileStatementNode(ExpressionNode exp, StatementNode state)
        {
            Exp = exp;
            State = state;
        }

        public ExpressionNode Exp { get; }
        public StatementNode State { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class AssignmentStatementNode : StatementNode
    {
        public AssignmentStatementNode(ExpressionNode varRef, Lex op, ExpressionNode exp)
        {
            Exp = exp;
            VarRef = varRef;
            Op = op;
        }

        public ExpressionNode VarRef { get; set; }
        public Lex Op { get; }
        public ExpressionNode Exp { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }//присвоение

    public class FunctionCallStatementNode : StatementNode
    {
        public FunctionCallStatementNode(CallNode node)
        {
            Name = node.Name;
            Args = node.Args;
        }

        public VarRefNode Name { get; }
        public List<ExpressionNode> Args { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }//выдов функ

    public StatementNode Statement()//грамматика
    {
        var state = StructuredStatement() ?? SimpleStatement();
        return state;
    }

    public StatementNode SimpleStatement()
    {
        var left = Expression();
        
        if (left is VarRefNode)
        {
            Lex op;
            if (_curLex.Is(LexOperator.AssignSub, LexOperator.Assign,
                    LexOperator.AssignAdd, LexOperator.AssignDiv, LexOperator.AssignMul))
            {
                op = _curLex;
                Eat();
            }
            else if (left is CallNode callNode) return new FunctionCallStatementNode(callNode);
            else
            {
                throw new SyntaxException(_curLex.Pos, "Illegal expression");
            }

            var exp = Expression();
            return new AssignmentStatementNode(left, op, exp);
        }
        
        throw new SyntaxException(_curLex.Pos, "Illegal statement");
    }

    public CompoundStatementNode CompoundStatement() //бегин
    {
        Require(LexKeywords.BEGIN);
        var states = new List<StatementNode>();
        while (true)
        {
            var checker = 0;
            while (_curLex.Is(LexSeparator.Semicolom))
            {
                checker++;
                Require(LexSeparator.Semicolom);
            }

            if (_curLex.Is(LexKeywords.END))
            {
                Eat();
                break;
            }

            if (checker == 0 && states.Count != 0)
                Require(LexSeparator.Semicolom);
            states.Add(Statement());
        }

        return new CompoundStatementNode(states);
    }

    public StatementNode? StructuredStatement()
    {
        if (_curLex.Is(LexKeywords.BEGIN)) return CompoundStatement();
        if (_curLex.Is(LexKeywords.WHILE)) return WhileStatement();
        if (_curLex.Is(LexKeywords.FOR)) return ForStatement();
        if (_curLex.Is(LexKeywords.IF)) return IfStatement();
        return null;
    }

    public ForStatementNode ForStatement()
    {
        Eat();
        var id = VarRef();
        Require(LexOperator.Assign);
        var expFor = Expression();
        Require(_curLex.Is(LexKeywords.TO) ? LexKeywords.TO : LexKeywords.DOWNTO, false);
        var to = Keyword();
        var expTo = Expression();
        Require(LexKeywords.DO);
        var state = Statement();
        return new ForStatementNode(id, expFor, to, expTo, state);
    }

    public WhileStatementNode WhileStatement()
    {
        Eat();
        var exp = Expression();
        Require(LexKeywords.DO);
        var state = Statement();
        return new WhileStatementNode(exp, state);
    }

    public IfStatementNode IfStatement()
    {
        Eat();
        var exp = Expression();
        Require(LexKeywords.THEN);
        var stateThen = Statement();
        StatementNode stateElse = null!;
        if (_curLex.Is(LexKeywords.ELSE))
        {
            Eat();
            stateElse = Statement();
        }

        return new IfStatementNode(exp, stateThen, stateElse);
    }
}