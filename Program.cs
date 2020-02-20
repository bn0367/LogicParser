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
                Expression e;
                if(inp != "random") 
                    e = Expression.Parse(inp);
                else
                    e = Expression.RandomExpression(5);
                Console.WriteLine();
                e.PrintTable();
                e.Prove();
                Console.WriteLine("Enter a logical expression (or exit to exit):");
            }
        }
    }
}