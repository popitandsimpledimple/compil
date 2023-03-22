using Lexer;
using Parser.Sym;
using Parser.Visitor;

namespace Parser;

public partial class Parser
{
    public SymType Type()
    {
        var lex = _curLex;
        if (lex.Is(LexType.Identifier) || lex.Is(LexKeywords.STRING)) return PrimitiveType();
        if (lex.Is(LexKeywords.ARRAY)) return ArrayType();

        if (lex.Is(LexKeywords.RECORD)) return RecordType();
        throw new SyntaxException(_curLex.Pos, "type expected");
    }

    public SymType PrimitiveType()
    {
        if (_curLex.Is(LexKeywords.STRING))
        {
            var lex = _curLex;
            Eat();
            return new SymType(lex.Value.ToString()!.ToLower());
        }

        return new SymType(Id().ToString());
    }

    public SymArray ArrayType()
    {
        Eat();
        Require(LexSeparator.Lbrack);
        var ranges = TypeRanges();
        Require(LexSeparator.Rbrack);
        Require(LexKeywords.OF);
        var type = Type();
        ranges.Reverse();
        foreach (var range in ranges)
        {
            type = new SymArray(type, range);
        }
        return (type as SymArray)!;
    }

    public List<TypeRangeNode> TypeRanges()
    {
        var ranges = new List<TypeRangeNode> {TypeRange()};
        while (true)
        {
            if (!_curLex.Is(LexSeparator.Comma)) break;

            Eat();
            ranges.Add(TypeRange());
        }

        return ranges;
    }

    public TypeRangeNode TypeRange()
    {
        var begin = Expression();
        Require(LexSeparator.Doubledot);
        var end = Expression();
        return new TypeRangeNode(begin, end);
    }

    public class TypeRangeNode : Node
    {
        public TypeRangeNode(ExpressionNode begin, ExpressionNode end)
        {
            Begin = begin;
            End = end;
        }

        public ExpressionNode Begin { get; }
        public ExpressionNode End { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public SymRecord RecordType()
    {
        Eat();
        var fieldList = new List<List<FieldSelectionNode>>();
        while (!_curLex.Is(LexKeywords.END)) fieldList.Add(FieldList());
        Require(LexKeywords.END);
        var table = new SymTable();
        foreach (var filds in fieldList)
        foreach (var field in filds)
        foreach (var idNode in field.Ids)
            table.Push(idNode, new SymVar(idNode, field.Type));
        return new SymRecord(table);
    }

    public List<FieldSelectionNode> FieldList()
    {
        var fields = new List<FieldSelectionNode> {FieldSelection()};
        while (true)
        {
            if (_curLex.Is(LexSeparator.Semicolom))
            {
                Eat();
                break;
            }

            Eat();
            fields.Add(FieldSelection());
        }

        return fields;
    }

    public FieldSelectionNode FieldSelection()
    {
        var ids = IdList();
        Require(LexSeparator.Colon);
        var type = Type();
        return new FieldSelectionNode(ids, type);
    }

    public class FieldSelectionNode : Node
    {
        public FieldSelectionNode(List<IdNode> ids, SymType type)
        {
            Ids = ids;
            Type = type;
        }

        public List<IdNode> Ids { get; }
        public SymType Type { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}