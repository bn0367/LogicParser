using Microsoft.Z3;
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
            while ((inp = Console.ReadLine()) != "exit")
            {
                //Expression e = Expression.Parse(inp);
                Expression e = Expression.RandomExpression(5);
                Console.WriteLine();
                e.PrintTable(e.GTable);
                e.Prove();
                Console.WriteLine("Enter a logical expression (or exit to exit):");
            }
        }
    }
}