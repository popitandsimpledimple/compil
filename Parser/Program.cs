using System.Globalization;
using Parser;
using Parser.Sym;
using Parser.Visitor;

internal class program
{
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        using (var fileReader = new StreamReader("../../../../test.in"))
        {
            var parser = new Parser.Parser(fileReader);
            try
            {
                var programNode = parser.Program();
                
                SymStack _symStack = new SymStack();
                SymVisitor visitorSym = new SymVisitor(_symStack);
                visitorSym.Visit(programNode);
                
                IVisitor visitor = new PrinterVisitor();
                visitor.Visit(programNode);
                _symStack.Print(_symStack);

            }
            catch (SyntaxException e)
            {
                Console.Write(e.Message);
            }
            catch (SemanticException e)
            {
                Console.Write(e.Message);
            }
        }
    }
}