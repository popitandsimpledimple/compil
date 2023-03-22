using System.Collections.Specialized;
using Lexer;
using Parser.Visitor;

namespace Parser.Sym;

public abstract class Sym : Parser.IAcceptable //симантические переменные работает как для парсера так и для семантики
{
    public Sym(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public abstract void Accept(IVisitor visitor);

    public override string ToString()
    {
        return Name;
    }
}

public class SymProgramName : SymType // табл, именно програм а не фун процедура
{
    public SymProgramName(string name) : base(name)
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "program name";
    }
}

public class SymType : Sym // типы
{
    public SymType(string name) : base(name)
    {
    }

    public virtual SymType ResolveAlias()
    {
        return this;
    }

    public virtual bool Is(SymType other)
    {
        return ResolveAlias().Name.Equals(other.ResolveAlias().Name);
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "type";
    }
}

public class SymInteger : SymType //
{
    public SymInteger() : base("integer")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymDouble : SymType //
{
    public SymDouble() : base("double")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymBoolean : SymType
{
    public SymBoolean() : base("boolean")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymChar : SymType
{
    public SymChar() : base("char")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymString : SymType
{
    public SymString() : base("string")
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymAlias : SymType //псевдонимы ? // присвоение типов 
{
    public SymType Original { get; }
    public Parser.IdNode Id { get; }

    public SymAlias(Parser.IdNode id, SymType original) : base(id.ToString())
    {
        Id = id;
        Original = original;
    }

    public override SymType ResolveAlias()
    {
        return Original;
    }

    public override void Accept(IVisitor visitor) 
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "alias";
    }
}

public class SymVar : Sym
{
    public SymType Type { get; set; }

    public SymVar(Parser.IdNode id, SymType type) : base(id.ToString())
    {
        Id = id;
        Type = type;
    }

    public Parser.IdNode Id { get; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "var";
    }
}

public class SymConst : SymVar
{
    public SymConst(Parser.IdNode id, SymType type) : base(id, type)
    {
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "const";
    }
}

public class SymVarParam : SymParam // var - параметра(фун и тд)
{
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public SymVarParam(Parser.IdNode id, SymType type) : base(id, type)
    {
        Id = id;
    }

    public Parser.IdNode Id { get; }

    public override string ToString()
    {
        return "var param";
    }
}

public class SymConstParam : SymParam
{
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public SymConstParam(Parser.IdNode id, SymType type) : base(id, type)
    {
        Id = id;
    }

    public Parser.IdNode Id { get; }

    public override string ToString()
    {
        return "const param";
    }
}

public class SymRecord : SymType // рекорд
{
    public SymTable Fields { get; }


    public SymRecord(SymTable fields) : base("record")
    {
        Fields = fields;
    }

    public override bool Is(SymType other) //это сим? да -> побежали
    {
        if (other is not SymRecord) return false;

        var otherCasted = other as SymRecord;
        foreach (string field in Fields.Data.Keys)
        {
            var sym = Fields.Get(field) as SymVar;
            if (!otherCasted!.Fields.Contains(sym!.Name)) return false;

            var otherSym = otherCasted.Fields.Get(sym.Name) as SymVar;

            if (!sym.Type.Is(otherSym!.Type)) return false;
        }

        return true;
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "record";
    }
}

public class SymArray : SymType
{
    public SymType Type { get; }
    public Parser.TypeRangeNode Range { get; }

    public SymArray(SymType type, Parser.TypeRangeNode range) : base("array")
    {
        Type = type;
        Range = range;
    }

    public override bool Is(SymType other)
    {
        var array = other as SymArray;
        return array != null && Type.Is(array.Type);
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "array";
    }
}

public class SymParam : SymVar
{
    public SymParam(Parser.IdNode id, SymType type) : base(id, type)
    {
        Id = id;
    }

    public Parser.IdNode Id;

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "param";
    }
}

public class SymProcedure : Sym // имя процедуры, в табл, в блоке бегает как обычная программа
{
    public SymProcedure(Parser.IdNode id, SymTable locals, Parser.BlockNode block) : base(id.ToString())
    {
        Id = id;
        Locals = locals;
        Block = block;
    }

    public Parser.IdNode Id { get; }
    public SymTable Locals { get; }
    public Parser.BlockNode Block { get; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "procedure";
    }
}

public class SymFunction : SymProcedure
{
    public SymFunction(Parser.IdNode id, SymTable locals, Parser.BlockNode block, SymType returnType) : base(id, locals,
        block)
    {
        ReturnType = returnType;
    }

    public SymType ReturnType { get; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "function";
    }
}

public class SymTable : Parser.IAcceptable
{
    public OrderedDictionary Data { get; }
    public Parser.IdNode Duplicate { get; set; }

    public SymTable()
    {
        Data = new OrderedDictionary();
        Duplicate = null!;
    }

    public void Push(Parser.IdNode id, Sym sym, bool isParser = false)
    {
        if (isParser && Contains(sym.Name))
        {
            Duplicate = id;
            return;
        }

        if (Contains(sym.Name)) throw new SemanticException(id.LexCur.Pos, $"Duplicate identifier '{sym.Name}'");

        Data.Add(sym.Name, sym);
    }

    public void Border(string[] texts, int wight)
    {
        var textInString = "";
        foreach (var text in texts)
        {
            var space = text.Length > wight / 4 ? 0 : wight / 4 - text.Length;

            textInString += " │ " + text + " ".PadRight(space);
        }

        Console.Out.Write(textInString);
        Console.WriteLine(new string(' ', wight - 1 - textInString.Length) + "│");
    }// таб

    public void Print(SymTable table, SymStack stack)
    {
        Console.WriteLine();
        const int wight = 100;
        Console.WriteLine(" " + new string('─', wight - 1));
        foreach (var key in table.Data.Keys)
        {
            var value = "no return type";
            var programName = table.Data[key] as SymProgramName;
            var call = table.Data[key] as SymFunction is null
                ? table.Data[key] as SymProcedure
                : table.Data[key] as SymFunction;
            if (call is not null)
            {
                stack.Push(call.Locals);
                if (call is SymFunction) value = (call as SymFunction).ReturnType.Name;
            }

            if (programName is null)
                if (call is null)
                    value = table.Data[key] as SymVar is null
                        ? ((table.Data[key] as SymType)!).ResolveAlias().Name
                        : ((table.Data[key] as SymVar)!).Type.ResolveAlias().Name;

            if (value is null) value = "no return type";

            string[] data = {key.ToString(), table.Data[key].ToString(), value};
            Border(data, wight);
        }

        Console.WriteLine(" " + new string('─', wight - 1));
    }// таб

    public void Push(string name, Sym sym)
    {
        if (Contains(name)) throw new Exception();

        Data.Add(name, sym);
    }

    public Sym? Get(string name)
    {
        if (Data.Contains(name)) return (Sym) Data[name]!;

        return null;
    }

    public bool Contains(string name)
    {
        return Data.Contains(name);
    }

    public void Alloc()
    {
        Data.Add("integer", new SymInteger());
        Data.Add("char", new SymChar());
        Data.Add("string", new SymString());
        Data.Add("double", new SymDouble());
        Data.Add("boolean", new SymBoolean());
    }

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public class SymStack
{
    public List<SymTable> Data { get; }

    public SymStack()
    {
        Data = new List<SymTable>();
    }

    public void Push(SymTable data)
    {
        Data.Add(data);
    }

    public void Alloc()
    {
        Data.Add(new SymTable());
    }

    public void AllocBuiltins()
    {
        var a = new SymTable();
        a.Alloc();
        Data.Add(a);
    }

    public void Push(Parser.IdNode id, Sym sym)
    {
        Data[^1].Push(id, sym);
    }

    public Sym? Get(Lex lex, string name)
    {
        for (var i = Data.Count - 1; i >= 0; i--)
            if (Data[i].Contains(name))
                return Data[i].Get(name);
        throw new SemanticException(lex.Pos, $"Identifier not found '{name}'");
    }

    public void Pop()
    {
        Data.Remove(Data[^1]);
    }

    public void Print(SymStack stack)
    {
        for (var i = 0; i < stack.Data.Count; ++i)
            if (stack.Data[i].Data.Count != 0)
                stack.Data[i].Print(stack.Data[i], stack);
    }
}