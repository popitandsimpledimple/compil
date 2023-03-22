using Parser.Sym;

namespace Parser.Visitor;

public interface IVisitor
{
    void Visit(Parser.ProgramNode node);
    
    void Visit(Parser.BinOpExpressionNode node);
    void Visit(Parser.UnOpExpressionNode node);
    void Visit(Parser.RelOpExpressionNode node);
    
    void Visit(Parser.RecordAccess node);
    void Visit(SymRecord node);
    
    void Visit(Parser.ArrayAccess node);
    void Visit(SymArray node);
    
    void Visit(Parser.CallNode node);
    void Visit(Parser.BlockNode node);
    void Visit(Parser.ConstDeclsNode node);
    void Visit(Parser.VarDeclsNode node);
    void Visit(Parser.TypeDeclsNode node);
    void Visit(SymFunction node);
    void Visit(SymProcedure node);
    void Visit(SymAlias node);
    void Visit(Parser.CompoundStatementNode node);
    void Visit(Parser.FunctionCallStatementNode node);
    void Visit(Parser.AssignmentStatementNode node);
    void Visit(Parser.IfStatementNode node);
    void Visit(Parser.WhileStatementNode node);
    void Visit(Parser.ForStatementNode node);
    void Visit(Parser.TypeRangeNode node);
    void Visit(Parser.FieldSelectionNode node);
    void Visit(Parser.ConstDeclNode node);
    void Visit(Parser.VarDeclNode node);
    void Visit(SymConstParam node);
    void Visit(SymVarParam node);
    void Visit(SymParam node);
    void Visit(SymVar node);
    void Visit(SymTable node);
    void Visit(Parser.ParamSelectionNode node);
    
    void Visit(Parser.NumberExpressionNode node);
    void Visit(Parser.StringNode node);
    void Visit(Parser.BooleanNode node);
    void Visit(Parser.IdNode node);
    
    void Visit(SymType node);
    void Visit(Parser.CharNode node);
    void Visit(Parser.CastNode node);
    void Visit(Parser.KeywordNode node);
    void Visit(SymInteger node);
    void Visit(SymDouble node);
    void Visit(SymChar node);
    void Visit(SymString node);
    void Visit(SymBoolean node);
    void Visit(SymConst node);
}