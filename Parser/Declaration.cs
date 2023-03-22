using Lexer;
using Parser.Sym;
using Parser.Visitor;

namespace Parser;

public partial class Parser
{
    public abstract class DeclarationNode : Node //описание грамматики
    {
    }

    public class BlockNode : Node
    {
        public BlockNode(List<IAcceptable> declarations, CompoundStatementNode compound)
        {
            Declarations = declarations;
            Compound = compound;
        }

        public List<IAcceptable> Declarations { get; }
        public CompoundStatementNode Compound { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class VarDeclNode : DeclarationNode
    {
        public VarDeclNode(List<IdNode> names, List<SymVar> symVars, ExpressionNode? exp)
        {
            Names = names;
            SymVars = symVars;
            Exp = exp;
        }

        public List<IdNode> Names { get; }
        public List<SymVar> SymVars { get; }
        public ExpressionNode? Exp { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class VarDeclsNode : DeclarationNode
    {
        public List<VarDeclNode> Decls { get; }

        public VarDeclsNode(List<VarDeclNode> decls)
        {
            Decls = decls;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ConstDeclsNode : DeclarationNode
    {
        public ConstDeclsNode(List<ConstDeclNode> decls)
        {
            Decls = decls;
        }

        public List<ConstDeclNode> Decls { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ConstDeclNode : DeclarationNode
    {
        public ConstDeclNode(IdNode name, SymConst symConst, ExpressionNode exp) 
        {
            Name = name;
            SymConst = symConst;
            Exp = exp;
        }

        public IdNode Name { get; }
        public SymConst SymConst { get; }
        public ExpressionNode Exp { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class TypeDeclsNode : DeclarationNode
    {
        public TypeDeclsNode(List<SymAlias> typeDecls)
        {
            TypeDecls = typeDecls;
        }

        public List<SymAlias> TypeDecls { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ParamSelectionNode : DeclarationNode
    {
        public ParamSelectionNode(KeywordNode modifier, List<IdNode> ids, SymType type)
        {
            Modifier = modifier;
            Ids = ids;
            Type = type;
        }

        public KeywordNode Modifier { get; }
        public List<IdNode> Ids { get; }
        public SymType Type { get; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    private ParamSelectionNode Parameter()
    {
        KeywordNode modifier = null!;
        if (_curLex.Is(LexKeywords.VAR, LexKeywords.CONST)) modifier = Keyword();
        var idList = IdList();
        Require(LexSeparator.Colon);
        return new ParamSelectionNode(modifier, idList, Type());
    }

    private SymFunction FuncDecl()
    {
        Eat();
        var id = Id();
        Require(LexSeparator.Lparen);
        var locals = new List<ParamSelectionNode>();
        if (!_curLex.Is(LexSeparator.Rparen))
        {
            locals.Add(Parameter());
            while (_curLex.Is(LexSeparator.Semicolom))
            {
                Eat();
                locals.Add(Parameter());
            }
        }

        Require(LexSeparator.Rparen);
        Require(LexSeparator.Colon);
        var type = Type();
        Require(LexSeparator.Semicolom);
        var decls = CallBlock();
        var compound = CompoundStatement();
        Require(LexSeparator.Semicolom);
        var table = new SymTable();
        foreach (var local in locals)
        foreach (var idNode in local.Ids)
            if (local.Modifier is null)
                table.Push(idNode, new SymParam(idNode, local.Type), true);
            else if (local.Modifier.LexCur.Is(LexKeywords.CONST))
                table.Push(idNode, new SymConstParam(idNode, local.Type), true);
            else if (local.Modifier.LexCur.Is(LexKeywords.VAR))
                table.Push(idNode, new SymVarParam(idNode, local.Type), true);
        return new SymFunction(id, table, new BlockNode(decls, compound), type);
    }

    private SymProcedure ProcDecl()
    {
        Eat();
        var id = Id();
        Require(LexSeparator.Lparen);
        var locals = new List<ParamSelectionNode>();
        if (!_curLex.Is(LexSeparator.Rparen))
        {
            locals.Add(Parameter());
            while (_curLex.Is(LexSeparator.Semicolom))
            {
                Eat();
                locals.Add(Parameter());
            }
        }

        Require(LexSeparator.Rparen);
        Require(LexSeparator.Semicolom);
        var decls = CallBlock();
        var compound = CompoundStatement();
        Require(LexSeparator.Semicolom);
        var table = new SymTable();
        foreach (var local in locals)
        foreach (var idNode in local.Ids)
            if (local.Modifier is null)
                table.Push(idNode, new SymParam(idNode, local.Type), true);
            else if (local.Modifier.LexCur.Is(LexKeywords.CONST))
                table.Push(idNode, new SymConstParam(idNode, local.Type), true);
            else if (local.Modifier.LexCur.Is(LexKeywords.VAR))
                table.Push(idNode, new SymVarParam(idNode, local.Type), true);

        return new SymProcedure(id, table, new BlockNode(decls, compound));
    }

    private List<IAcceptable> CallBlock()
    {
        var declarations = new List<IAcceptable>();
        while (true)
            if (_curLex.Is(LexKeywords.VAR))
                declarations.Add(VarDecls());
            else if (_curLex.Is(LexKeywords.TYPE))
                declarations.Add(TypeDecls());
            else if (_curLex.Is(LexKeywords.CONST))
                declarations.Add(ConstDecls());
            else
                break;
        return declarations;
    }

    private BlockNode Block()
    {
        var declarations = new List<IAcceptable>();
        while (true)
            if (_curLex.Is(LexKeywords.VAR))
                declarations.Add(VarDecls());
            else if (_curLex.Is(LexKeywords.TYPE))
                declarations.Add(TypeDecls());
            else if (_curLex.Is(LexKeywords.CONST))
                declarations.Add(ConstDecls());
            else if (_curLex.Is(LexKeywords.PROCEDURE))
                declarations.Add(ProcDecl());
            else if (_curLex.Is(LexKeywords.FUNCTION))
                declarations.Add(FuncDecl());
            else
                break;
        var compound = CompoundStatement();
        return new BlockNode(declarations, compound);
    }

    private ConstDeclsNode ConstDecls()
    {
        Eat();
        var decls = new List<ConstDeclNode> {ConstDecl()};
        while (_curLex.Is(LexType.Identifier)) decls.Add(ConstDecl());

        return new ConstDeclsNode(decls);
    }

    private ConstDeclNode ConstDecl()
    {
        var id = Id();
        SymType type = null!;
        if (_curLex.Is(LexSeparator.Colon))
        {
            Eat();
            type = Type();
        }

        Require(LexOperator.Equal);
        var exp = Expression();
        Require(LexSeparator.Semicolom);
        return new ConstDeclNode(id, new SymConst(id, type), exp);
    }

    private SymAlias TypeDecl()
    {
        var id = Id();
        Require(LexOperator.Equal);
        var type = Type();
        Require(LexSeparator.Semicolom);
        return new SymAlias(id, type);
    }

    private TypeDeclsNode TypeDecls()
    {
        Eat();
        var decls = new List<SymAlias> {TypeDecl()};
        while (_curLex.Is(LexType.Identifier)) decls.Add(TypeDecl());

        return new TypeDeclsNode(decls);
    }

    private VarDeclsNode VarDecls()
    {
        Eat();
        var decls = new List<VarDeclNode> {VarDecl()};
        while (_curLex.Is(LexType.Identifier)) decls.Add(VarDecl());

        return new VarDeclsNode(decls);
    }

    private VarDeclNode VarDecl()
    {
        var names = new List<IdNode>();
        var symVarParam = new List<SymVar>();
        names.Add(Id());
        while (_curLex.Is(LexSeparator.Comma))
        {
            Eat();
            names.Add(Id());
        }

        Require(LexSeparator.Colon);
        var type = Type();
        foreach (var i in names) symVarParam.Add(new SymVar(i, type));

        ExpressionNode? exp = null;
        if (_curLex.Is(LexOperator.Equal))
        {
            if (names.Count > 1) throw new SyntaxException(_curLex.Pos, "Error initialization");
            Eat();
            exp = Expression();
        }

        Require(LexSeparator.Semicolom);
        return new VarDeclNode(names, symVarParam, exp);
    }
}