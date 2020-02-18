using System;

namespace LogicParser
{

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter a logical expression: ");
                Expression l = Expression.Parse(Console.ReadLine());
                Console.WriteLine();
                l.PrintTable();
                Console.WriteLine();
            }
        }
    }
}