using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Lexer;
using Parser;
using Parser.Sym;
using Parser.Visitor;

namespace ChekerOut;

internal class Program
{
    private static int total = 0;
    private static int test = 0;
    public static bool SimpleParser(string pathFile) //проверка теста простой парсер
    {
        test++;
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var parser = new global::SimpleParser.SimpleParser(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        var line = fileReaderOut.ReadToEnd();
        var oldOut = Console.Out;
        try
        {
            var sw = new StringWriter();
            Console.SetOut(sw);
            parser.ParseExpression().PrintTree("");
            Console.SetOut(oldOut);
            var found = sw.ToString();
            if (line + "\r\n" != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }
        catch (SyntaxException e)
        {
            Console.SetOut(oldOut);
            var found = e.Message;
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }

        Console.WriteLine($"TEST {test} OK");
        return true;
    }
    public static bool Parser(string pathFile) // делает дерево
    {
        test++;
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var parser = new Parser.Parser(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        var line = fileReaderOut.ReadToEnd();
        var oldOut = Console.Out;
        try
        {
            var sw = new StringWriter();
            Console.SetOut(sw);
            IVisitor visitor = new PrinterVisitor();
            visitor.Visit(parser.Program());
            Console.SetOut(oldOut);
            var found = sw.ToString();
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }
        catch (SyntaxException e)
        {
            Console.SetOut(oldOut);
            var found = e.Message;
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }

        Console.WriteLine($"TEST {test} OK");
        return true;
    }
    
    public static bool Semantic(string pathFile)
    {
        test++;
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var parser = new Parser.Parser(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        var line = fileReaderOut.ReadToEnd();
        var oldOut = Console.Out;
        try
        {
            var sw = new StringWriter();
            Console.SetOut(sw);
            
            var programNode = parser.Program(); // таблица перменных печетает 
            SymStack symStack = new SymStack();
            SymVisitor visitorSym = new SymVisitor(symStack);
            visitorSym.Visit(programNode);
                
            IVisitor visitor = new PrinterVisitor(); // дерево
            visitor.Visit(programNode);
            symStack.Print(symStack);
            
            Console.SetOut(oldOut);
            var found = sw.ToString();
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }
        catch (SemanticException e) // ошибка
        {
            Console.SetOut(oldOut);
            var found = e.Message;
            if (line != found)
            {
                Console.WriteLine(pathFile + '\t' + "WA");
                Console.WriteLine("expected:\n" + line);
                Console.WriteLine("found:\n" + found);
                return false;
            }
        }

        Console.WriteLine($"TEST {test} OK");
        return true;
    }
    public static bool Lexer(string pathFile)
    {
        test++;
        var fileReaderIn = new StreamReader(pathFile + ".in");
        var scan = new Scanner(fileReaderIn);
        var fileReaderOut = new StreamReader(pathFile + ".out");
        do
        {
            string line;
            line = fileReaderOut.ReadLine();
            try
            {
                var found = scan.ScannerLex().ToString();
                if (line != found)
                {
                    Console.WriteLine(pathFile + '\t' + "WA");
                    Console.WriteLine("expected:\n" + line);
                    Console.WriteLine("found:\n" + found + "\n");
                    return false;
                }
            }
            catch (LexException e)
            {
                var found = e.Message;
                if (line != found)
                {
                    Console.WriteLine(pathFile + '\t' + "WA");
                    Console.WriteLine("expected:\n" + line);
                    Console.WriteLine("found:\n" + found + "\n");
                    return false;
                }
                break;
            }
        } while (!fileReaderIn.EndOfStream);
        Console.WriteLine($"TEST {test} OK");
        return true;
    }
   

    public static void StartTest(string program)
    {
        var files = Directory.GetFiles($"../../../../Tests/{program}/", "*.in", SearchOption.AllDirectories)
            .Select(f => f[..^3]).ToList();


        Console.WriteLine($"{program} Test:");
        foreach (var file in files)
        {
            var check = false;
            if (program == "Lexer")
                check = Lexer(file);
            else if (program == "SimpleParser")
                check = SimpleParser(file);
            else if (program == "Parser")
                check = Parser(file);
            else if (program == "Semantic")
                check = Semantic(file);
            
            if (check) total++;
        }
    }
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        StartTest("Lexer");
        StartTest("SimpleParser");
        StartTest("Parser");
        StartTest("Semantic");
        Console.Out.Write($"TOTAL: {total}/{test}");
    }
}