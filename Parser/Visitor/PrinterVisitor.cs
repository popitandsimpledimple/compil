using Lexer;
using Parser.Sym;

namespace Parser.Visitor;

public class PrinterVisitor : IVisitor //дерево рисует
{
    private int _depth;

    private void PrintDepth()
    {
        Console.Write("".PadRight(_depth * 3, ' '));
    }

    private void Print(string str)
    {
        PrintDepth();
        Console.WriteLine(str);
    }

    public void Print(IEnumerable<Parser.IAcceptable> nodes)
    {
        foreach (var node in nodes) node.Accept(this);
    }

    public void Print(Lex lex)
    {
        PrintDepth();
        Console.WriteLine(lex.Source.ToLower());
    }

    public void Visit(Parser.ProgramNode node)
    {
        Print("program");
        node.Name?.Accept(this);
        node.Block.Accept(this);
    }

    public void Visit(Parser.BinOpExpressionNode node)
    {
        _depth++;
        Print(node.Op);
        node.Left.Accept(this);
        node.Right.Accept(this);
        _depth--;
    }

    public void Visit(Parser.RelOpExpressionNode node)
    {
        _depth++;
        Print(node.Op);
        node.Left.Accept(this);
        node.Right.Accept(this);
        _depth--;
    }

    public void Visit(Parser.CharNode node)
    {
        _depth++;
        Print(node.LexCur);
        _depth--;
    }

    public void Visit(Parser.CastNode node)
    {
    }

    public void Visit(Parser.UnOpExpressionNode node)
    {
        _depth++;
        Print(node.Op);
        node.Operand.Accept(this);
        _depth--;
    }

    public void Visit(Parser.RecordAccess node)
    {
        _depth++;
        Print("record access");
        node.RecordVarRef.Accept(this);
        node.Field.Accept(this);
        _depth--;
    }

    public void Visit(Parser.ArrayAccess node)
    {
        _depth++;
        Print("array access");
        node.ArrayId.Accept(this);
        node.ArrayExp.Accept(this);
        _depth--;
    }

    public void Visit(Parser.CallNode node)
    {
        _depth++;
        Print("call");
        node.Name.Accept(this);
        Print(node.Args);
        _depth--;
    }

    public void Visit(Parser.NumberExpressionNode node)
    {
        _depth++;
        Print(node.LexCur);
        _depth--;
    }

    public void Visit(Parser.StringNode node)
    {
        _depth++;
        Print(node.LexCur);
        _depth--;
    }

    public void Visit(Parser.BooleanNode node)
    {
        _depth++;
        Print(node.LexCur);
        _depth--;
    }

    public void Visit(Parser.IdNode node)
    {
        _depth++;
        Print(node.LexCur);
        _depth--;
    }

    public void Visit(SymInteger node)
    {
        _depth++;
        Print(node.Name);
        _depth--;
    }

    public void Visit(SymDouble node)
    {
        _depth++;
        Print(node.Name);
        _depth--;
    }

    public void Visit(SymChar node)
    {
        _depth++;
        Print(node.Name);
        _depth--;
    }

    public void Visit(SymString node)
    {
        _depth++;
        Print(node.Name);
        _depth--;
    }

    public void Visit(SymBoolean node)
    {
        _depth++;
        Print(node.Name);
        _depth--;
    }

    public void Visit(SymConst node)
    {
        _depth++;
        Print(node.Name);
        if (node.Type is not null)
            node.Type.Accept(this);
        else
            Print("no type");

        _depth--;
    }

    public void Visit(Parser.BlockNode node)
    {
        _depth++;
        if (node.Declarations.Count != 0)
        {
            Print("decls");
            Print(node.Declarations);
        }

        Print("block");
        node.Compound.Accept(this);
        _depth--;
    }

    public void Visit(Parser.ConstDeclsNode node)
    {
        _depth++;
        Print("consts");
        Print(node.Decls);
        _depth--;
    }

    public void Visit(Parser.ConstDeclNode node)
    {
        _depth++;
        Print("const");
        node.SymConst.Accept(this);
        if (node.Exp is not null)
            node.Exp.Accept(this);
        else
            Print("no expression");

        _depth--;
    }

    public void Visit(Parser.VarDeclsNode node)
    {
        _depth++;
        Print("vars");
        Print(node.Decls);
        _depth--;
    }

    public void Visit(Parser.VarDeclNode node)
    {
        _depth++;

        Print("var");
        Print(node.Names);
        _depth++;
        node.SymVars[0].Type.Accept(this);
        if (node.Exp is not null)
            node.Exp.Accept(this);
        else
            Print("no expression");

        _depth--;
        _depth--;
    }

    public void Visit(Parser.TypeDeclsNode node)
    {
        _depth++;
        Print("decls types");
        Print(node.TypeDecls);
        _depth--;
    }

    public void Visit(SymFunction node)
    {
        _depth++;
        Print("function");
        _depth++;
        Print(node.Name);
        _depth--;
        node.ReturnType.Accept(this);
        node.Locals.Accept(this);
        node.Block.Accept(this);
        _depth--;
    }

    public void Visit(SymProcedure node)
    {
        _depth++;
        Print("procedure");
        _depth++;
        Print(node.Name);
        _depth--;
        node.Locals.Accept(this);
        node.Block.Accept(this);
        _depth--;
    }

    public void Visit(SymAlias node)
    {
        _depth++;
        Print("decl type");
        _depth++;
        Print(node.Name);
        node.Original.Accept(this);
        _depth--;
        _depth--;
    }

    public void Visit(Parser.KeywordNode node)
    {
        _depth++;
        Print(node.LexCur);
        _depth--;
    }

    public void Visit(Parser.CompoundStatementNode node)
    {
        _depth++;
        Print("compounds statements");
        Print(node.States);
        _depth--;
    }

    public void Visit(Parser.FunctionCallStatementNode node)
    {
        _depth++;
        Print("call");
        node.Name.Accept(this);
        _depth++;
        Print(node.Args);
        _depth--;
        _depth--;
    }

    public void Visit(Parser.AssignmentStatementNode node)
    {
        _depth++;
        Print(node.Op);
        node.VarRef.Accept(this);
        node.Exp.Accept(this);
        _depth--;
    }

    public void Visit(Parser.IfStatementNode node)
    {
        _depth++;
        Print("if");
        node.Exp.Accept(this);
        node.StateThen.Accept(this);
        Print("else");
        if (node.StateElse is not null)
        {
            node.StateElse.Accept(this);
        }
        else
        {
            _depth++;
            Print("empty");
            _depth--;
        }

        _depth--;
    }

    public void Visit(Parser.WhileStatementNode node)
    {
        _depth++;
        Print("while");
        node.Exp.Accept(this);
        node.State.Accept(this);
        _depth--;
    }

    public void Visit(Parser.ForStatementNode node)
    {
        _depth++;
        Print("for");
        Print(":=");
        node.Id.Accept(this);
        node.ExpFor.Accept(this);
        _depth--;
        node.To.Accept(this);
        _depth++;
        _depth++;
        node.ExpTo.Accept(this);
        _depth--;
        node.State.Accept(this);
        _depth--;
    }

    public void Visit(SymType node)
    {
        Print("type");
        _depth++;
        Print(node.Name);
        _depth--;
    }

    public void Visit(SymArray node)
    {
        _depth++;
        Print(node.Name);
        node.Range.Accept(this);
        _depth--;
        node.Type.Accept(this);
    }

    public void Visit(SymRecord node)
    {
        _depth++;
        Print(node.Name);
        node.Fields.Accept(this);
        _depth--;
    }

    public void Visit(Parser.TypeRangeNode node)
    {
        _depth++;
        Print("range");
        _depth++;
        Print("..");
        node.Begin.Accept(this);
        node.End.Accept(this);
        _depth--;
        _depth--;
    }

    public void Visit(Parser.FieldSelectionNode node)
    {
        _depth++;
        Print("fields");
        node.Type.Accept(this);
        Print(node.Ids);
        _depth--;
    }

    public void Visit(SymConstParam node)
    {
        Print("const");
        _depth++;
        Print(node.Name);
        node.Type.Accept(this);
        _depth--;
    }

    public void Visit(SymVarParam node)
    {
        Print("var");
        _depth++;
        Print(node.Name);
        node.Type.Accept(this);
        _depth--;
    }

    public void Visit(SymParam node)
    {
        Print("param");
        _depth++;
        Print(node.Name);
        node.Type.Accept(this);
        _depth--;
    }

    public void Visit(SymVar node)
    {
        _depth++;
        Print(node.Name);
        if (node.Type is not null)
            node.Type.Accept(this);
        else
            Print("no type");

        _depth--;
    }

    public void Visit(SymTable node)
    {
        Print("params");
        _depth++;
        foreach (var key in node.Data.Keys) (node.Data[key] as SymVar)!.Accept(this);

        _depth--;
    }

    public void Visit(Parser.ParamSelectionNode node)
    {
        _depth++;
        Print("param");
        if (node.Modifier is null)
            node.Modifier.Accept(this);
        else
            Print("no modifier");

        Print(node.Ids);
        node.Type.Accept(this);
        _depth--;
    }
}