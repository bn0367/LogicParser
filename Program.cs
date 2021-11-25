using System;
using System.Linq;
namespace LogicParser
{

    class Program
    {
        static void Main(string[] _)
        {
            Console.WriteLine("Enter a logical expression (or exit to exit):");
            string inp;
            Expression e;
            while ((inp = Console.ReadLine()) != "exit")
            {
                if (inp != "random")
                    e = Expression.Parse(inp);
                else
                    e = Expression.RandomExpression(5);
                Console.WriteLine();
                e.PrintTable();
                e.Prove();
                e.Simplify();
                Console.WriteLine("Enter a logical expression (or exit to exit):");
            }
        }
    }
}