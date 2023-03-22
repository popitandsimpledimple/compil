using Lexer;

namespace Parser;

public partial class Parser //утилиты
{
    public List<IdNode> IdList()
    {
        var ids = new List<IdNode>();

        while (true)
        {
            ids.Add(Id());
            if (!_curLex.Is(LexSeparator.Comma))
                break;
            Eat();
        }

        if (ids == null)
            throw new("нулевой идентификатор");
        
        return ids;
    }
    public List<ExpressionNode> ExpressionList()
    {
        var exps = new List<ExpressionNode>();

        while (true)
        {
            exps.Add(Expression());
            if (!_curLex.Is(LexSeparator.Comma))
                break;
            Eat();
        }

        if (exps == null)
            throw new("null expression"); //нулевое выражение


        return exps;
    }
}